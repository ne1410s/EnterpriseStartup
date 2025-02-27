// <copyright file="TestObjects.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Messaging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging;
using EnterpriseStartup.Telemetry;
using RabbitMQ.Client;

public record BasicPayload(string Foo, bool? SimulateRetry);

public class BasicTracedProducer(
    IConnectionFactory connectionFactory,
    ITelemeter telemeter,
    ILogger<BasicTracedProducer> logger)
        : MqTracingProducer<BasicPayload>(connectionFactory, telemeter, logger)
{
    public override string ExchangeName => TestHelper.TestExchangeName;
}

public class BasicTracedConsumer : MqTracingConsumer<BasicPayload>
{
    public BasicTracedConsumer(
        IConnectionFactory connectionFactory,
        ITelemeter telemeter,
        ILogger<BasicTracedConsumer> logger,
        IConfiguration config)
            : base(connectionFactory, telemeter, logger, config)
    {
        this.StartInternal(CancellationToken.None);
    }

    public override string ExchangeName => TestHelper.TestExchangeName;

    public override Task ConsumeAsync(BasicPayload message, MqConsumerEventArgs args)
        => Task.CompletedTask;
}