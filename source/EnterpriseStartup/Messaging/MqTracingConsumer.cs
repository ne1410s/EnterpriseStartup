﻿// <copyright file="MqTracingConsumer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging;

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging.RabbitMq;
using EnterpriseStartup.Telemetry;
using FluentErrors.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;

/// <inheritdoc cref="RabbitMqConsumer{T}"/>
public abstract class MqTracingConsumer<T> : RabbitMqConsumer<T>
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly ITelemeter telemeter;
    private readonly ILogger<MqTracingConsumer<T>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqTracingConsumer{T}"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    /// <param name="telemeter">The telemeter.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="config">The config.</param>
    protected MqTracingConsumer(
        IConnectionFactory connectionFactory,
        ITelemeter telemeter,
        ILogger<MqTracingConsumer<T>> logger,
        IConfiguration config)
            : base(connectionFactory)
    {
        this.telemeter = telemeter;
        this.logger = logger;

        config.MustExist();

        this.ConsumerAppName = config["RabbitMq:ConsumerAppName"];
        this.ConsumerAppName.MustBePopulated();
        this.MaximumAttempts = this.GetConfigValue<long>(config, nameof(this.MaximumAttempts));

        this.Starting += this.OnStarting;
        this.Started += this.OnStarted;
        this.Stopping += this.OnStopping;
        this.Stopped += this.OnStopped;

        this.MessageReceived += this.OnMessageReceived;
        this.MessageProcessed += this.OnMessageProcessed;
        this.MessageFailed += this.OnMessageFailed;
    }

    [ExcludeFromCodeCoverage]
    private static PropagationContext GetPropagationContext(MqEventArgs e)
    {
        return Propagator.Extract(default, e.Headers, (carrier, key) =>
        {
            var value = carrier.TryGetValue(key, out var val) ? (byte[])val : null;
            return value == null ? null : [Encoding.UTF8.GetString(value)];
        });
    }

    private TProp? GetConfigValue<TProp>(IConfiguration config, string property)
        where TProp : struct
    {
        var queueValue = config.GetValue<TProp?>($"RabbitMq:Queues:{this.QueueName}:{property}");
        var exchangeValue = config.GetValue<TProp?>($"RabbitMq:Exchanges:{this.ExchangeName}:{property}");
        return queueValue ?? exchangeValue;
    }

    private void OnStarting(object? sender, System.EventArgs e)
        => this.logger.LogInformation("Mq consumer starting: {Queue}", this.QueueName);

    private void OnStarted(object? sender, System.EventArgs e)
        => this.logger.LogInformation("Mq consumer started: {Queue}", this.QueueName);

    private void OnStopping(object? sender, System.EventArgs e)
        => this.logger.LogInformation("Mq consumer stopping: {Queue}", this.QueueName);

    private void OnStopped(object? sender, System.EventArgs e)
        => this.logger.LogInformation("Mq consumer stopped: {Queue}", this.QueueName);

    private void OnMessageReceived(object? sender, MqConsumerEventArgs e)
    {
        this.logger.LogInformation(
            "Mq message incoming: {Queue}@{BornOn} ({Attempt}x)",
            this.QueueName,
            e.BornOn,
            e.AttemptNumber);

        var tags = new KeyValuePair<string, object?>[]
        {
            new("queue", this.QueueName),
            new("born", e.BornOn),
            new("json", e.Message),
            new("attempt", e.AttemptNumber),
        };

        var parentContext = GetPropagationContext(e);
        Baggage.Current = parentContext.Baggage;
        using var activity = this.telemeter.StartTrace(
            "mq_consume",
            ActivityKind.Consumer,
            parentContext.ActivityContext,
            tags);
    }

    private void OnMessageFailed(object? sender, MqFailedEventArgs e)
    {
        var outcome = e.Retry == false ? "permanent error" : "transient error";
        this.logger.Log(
            e.Retry == false ? LogLevel.Error : LogLevel.Warning,
            e.Error,
            "Mq {Outcome}: {Queue}#{BornOn} ({Attempt}x)",
            outcome,
            this.QueueName,
            e.BornOn,
            e.AttemptNumber);

        var tags = new KeyValuePair<string, object?>[]
        {
            new("queue", this.QueueName),
            new("outcome", outcome),
        };

        this.telemeter.CaptureMetric(MetricType.Counter, 1, "mq_consume_failure", tags: tags);
    }

    private void OnMessageProcessed(object? sender, MqConsumerEventArgs e)
    {
        this.logger.LogInformation(
            "Mq message success: {Queue}#{BornOn} ({Attempt}x)",
            this.QueueName,
            e.BornOn,
            e.AttemptNumber);

        var tags = new KeyValuePair<string, object?>[]
        {
            new("queue", this.QueueName),
            new("outcome", "success"),
        };

        this.telemeter.CaptureMetric(MetricType.Counter, 1, "mq_consume_success", tags: tags);
    }
}
