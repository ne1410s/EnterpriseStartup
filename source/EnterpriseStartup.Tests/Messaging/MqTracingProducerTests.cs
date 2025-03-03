﻿// <copyright file="MqTracingProducerTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Messaging;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnterpriseStartup.Messaging.Abstractions.Producer;
using EnterpriseStartup.Messaging;
using EnterpriseStartup.Telemetry;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="MqTracingProducer{T}"/> class.
/// </summary>
public class MqTracingProducerTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    [Fact]
    public void Produce_WhenCalled_WritesLogs()
    {
        // Arrange
        var sut = GetSut<BasicTracedProducer>(out var mocks);

        // Act
        sut.Produce(new("bar", true));

        // Assert
        mocks.MockLogger.VerifyLog(msgCheck: s => s == "Mq message sending: " + sut.ExchangeName);
        mocks.MockLogger.VerifyLog(msgCheck: s => s == "Mq message sent: " + sut.ExchangeName);
    }

    [Fact]
    public void ProduceAsync_WhenCalled_TracesActivity()
    {
        // Arrange
        var sut = GetSut<BasicTracedProducer>(out var mocks);
        var payload = new BasicPayload("bar", false);
        var tags = new KeyValuePair<string, object?>[]
        {
            new("exchange", sut.ExchangeName),
            new("json", ToJson(payload)),
        };

        // Act
        sut.Produce(payload);

        // Assert
        mocks.MockTelemeter.Verify(m => m.StartTrace("mq_produce", ActivityKind.Producer, default, tags));
    }

    [Fact]
    public void ProduceAsync_WithActivity_TracesActivity()
    {
        // Arrange
        const string name = "mq_produce";
        const ActivityKind kind = ActivityKind.Producer;
        var sut = GetSut<BasicTracedProducer>(out var mocks);
        var payload = new BasicPayload("bar", false);
        using var mockActivity = new Activity("whoop");
        mocks.MockTelemeter
            .Setup(m => m.StartTrace(name, kind, default, It.IsAny<KeyValuePair<string, object?>[]>()))
            .Returns(mockActivity);

        // Act
        sut.Produce(payload);

        // Assert
        mocks.MockTelemeter.Verify(m => m.StartTrace(name, kind, default, It.IsAny<KeyValuePair<string, object?>[]>()));
    }

    [Fact]
    public void ProduceAsync_WhenCalled_CapturesMetric()
    {
        // Arrange
        var sut = GetSut<BasicTracedProducer>(out var mocks);
        var payload = new BasicPayload("bar", null);
        var tags = new KeyValuePair<string, object?>[]
        {
            new("exchange", sut.ExchangeName),
            new("json", ToJson(payload)),
        };

        // Act
        sut.Produce(payload);

        // Assert
        mocks.MockTelemeter.Verify(
            m => m.CaptureMetric(MetricType.Counter, 1, "mq_produce", null, null, null, tags));
    }

    private static string ToJson(object obj)
        => JsonSerializer.Serialize(obj, JsonOpts);

    private static T GetSut<T>(out BagOfMocks<T> mocks)
        where T : IMqProducer
    {
        mocks = new(
            new Mock<IModel>(),
            new Mock<ITelemeter>(),
            new Mock<ILogger<T>>());

        var mockProps = new Mock<IBasicProperties>();
        mockProps
            .Setup(m => m.Headers)
            .Returns(new Dictionary<string, object>());

        mocks.MockChannel
            .Setup(m => m.CreateBasicProperties())
            .Returns(mockProps.Object);

        var mockConnection = new Mock<IConnection>();
        mockConnection
            .Setup(m => m.CreateModel())
            .Returns(mocks.MockChannel.Object);

        var mockConnectionFactory = new Mock<IConnectionFactory>();
        mockConnectionFactory
            .Setup(m => m.CreateConnection())
            .Returns(mockConnection.Object);

        return (T)Activator.CreateInstance(
            typeof(T),
            mockConnectionFactory.Object,
            mocks.MockTelemeter.Object,
            mocks.MockLogger.Object)!;
    }

    private sealed record BagOfMocks<T>(
        Mock<IModel> MockChannel,
        Mock<ITelemeter> MockTelemeter,
        Mock<ILogger<T>> MockLogger);
}
