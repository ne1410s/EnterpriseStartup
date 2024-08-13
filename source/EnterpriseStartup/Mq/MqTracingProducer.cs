// <copyright file="MqTracingProducer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Mq;

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging.RabbitMq;
using EnterpriseStartup.Telemetry;
using RabbitMQ.Client;

/// <inheritdoc cref="RabbitMqProducer{T}"/>
public abstract class MqTracingProducer<T> : RabbitMqProducer<T>
{
    private readonly ITelemeter telemeter;
    private readonly ILogger<MqTracingProducer<T>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqTracingProducer{T}"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    /// <param name="telemeter">The telemeter.</param>
    /// <param name="logger">The logger.</param>
    protected MqTracingProducer(
        IConnectionFactory connectionFactory,
        ITelemeter telemeter,
        ILogger<MqTracingProducer<T>> logger)
            : base(connectionFactory)
    {
        this.telemeter = telemeter;
        this.logger = logger;

        this.MessageSending += this.OnMessageSending;
        this.MessageSent += this.OnMessageSent;
    }

    private void OnMessageSending(object? sender, MqEventArgs e)
        => this.logger.LogInformation("Mq message sending: {Exchange}", this.ExchangeName);

    private void OnMessageSent(object? sender, MqEventArgs e)
    {
        this.logger.LogInformation("Mq message sent: {Exchange}", this.ExchangeName);

        var tags = new KeyValuePair<string, object?>[]
        {
            new("exchange", this.ExchangeName),
            new("json", e.Message),
        };

        this.telemeter.CaptureMetric(MetricType.Counter, 1, "mq-produce", tags: tags);
        using var activity = this.telemeter.StartTrace("mq-produce", tags: tags);
    }
}
