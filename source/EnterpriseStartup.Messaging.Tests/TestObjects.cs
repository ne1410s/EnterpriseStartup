﻿// <copyright file="TestObjects.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Tests;

using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

/// <summary>
/// A basic payload.
/// </summary>
/// <param name="PermaFail">Whether to fail permanently.</param>
public record BasicPayload(bool? PermaFail);

/// <summary>
/// A basic producer.
/// </summary>
/// <param name="factory">The connection factory.</param>
public class BasicProducer(IConnectionFactory factory)
    : RabbitMqProducer<BasicPayload>(factory)
{
    public override string ExchangeName => TestHelper.TestExchangeName;
}

public class BasicConsumer : RabbitMqConsumer<BasicPayload>
{
    public BasicConsumer(IConnectionFactory factory)
        : base(factory)
    {
        this.StartInternal(CancellationToken.None);
    }

    public Collection<string> Lifecycle { get; } = [];

    public override string ExchangeName => TestHelper.TestExchangeName;

    public override Task ConsumeAsync(BasicPayload message, MqConsumerEventArgs args)
    {
        return message.PermaFail switch
        {
            true => throw new PermanentFailureException(),
            false => throw new TransientFailureException(),
            _ => Task.CompletedTask,
        };
    }

    public async Task TestConsumerReceipt(object sender, BasicDeliverEventArgs args)
        => await this.OnConsumerReceipt(sender, args);
}

public class GenericConsumer : MqConsumerBase<BasicPayload>
{
    public GenericConsumer(long? maximumAttempts = null)
    {
        this.MaximumAttempts = maximumAttempts;
        this.ConsumerAppName = "PascalCase";
    }

    public Collection<string> Lifecycle { get; } = [];

    public override string ExchangeName => TestHelper.TestExchangeName;

    public override bool IsConnected => this.Lifecycle.Contains("StartInternal");

    public override Task ConsumeAsync(BasicPayload message, MqConsumerEventArgs args)
    {
        return message.PermaFail switch
        {
            true => throw new PermanentFailureException(),
            false => throw new TransientFailureException(),
            _ => Task.CompletedTask,
        };
    }

    public async Task TestConsume(BasicPayload payload, MqConsumerEventArgs args)
        => await this.TestConsume(JsonSerializer.Serialize(payload), args);

    public async Task TestConsume(string json, MqConsumerEventArgs args)
        => await this.ConsumeInternal(Encoding.UTF8.GetBytes(json), args);

    public bool TestDoRetry(MqFailedEventArgs args) => this.DoRetry(args);

    protected override Task StartInternal(CancellationToken token)
    {
        this.Lifecycle.Add("StartInternal");
        return Task.CompletedTask;
    }

    protected override Task StopInternal(CancellationToken token)
    {
        this.Lifecycle.Add("StopInternal");
        return Task.CompletedTask;
    }
}
