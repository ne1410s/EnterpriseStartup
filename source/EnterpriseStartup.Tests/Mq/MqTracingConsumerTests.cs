﻿// <copyright file="MqTracingConsumerTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Mq;

using System.Diagnostics;
using FluentErrors.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging;
using EnterpriseStartup.Telemetry;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="MqTracingConsumer{T}"/> class.
/// </summary>
public class MqTracingConsumerTests
{
    [Fact]
    public void Ctor_BothMaxAttemptsConfigured_TakesQueueValue()
    {
        // Arrange
        const int qValue = 10;
        const int xValue = 20;

        // Act
        var sut = GetSut<BasicTracedConsumer>(out _, "test", qValue, xValue);

        // Assert
        sut.MaximumAttempts.ShouldBe(qValue);
    }

    [Fact]
    public void Ctor_OnlyExchangeMaxAttemptsConfigured_TakesValue()
    {
        // Arrange
        var qValue = (int?)null;
        const int xValue = 20;

        // Act
        var sut = GetSut<BasicTracedConsumer>(out _, "test", qValue, xValue);

        // Assert
        sut.MaximumAttempts.ShouldBe(xValue);
    }

    [Fact]
    public void Ctor_NullConfig_ThrowsException()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockFactory = new Mock<IConnectionFactory>();
        _ = mockFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);

        // Act
        var act = () => new BasicTracedConsumer(
            mockFactory.Object,
            new Mock<ITelemeter>().Object,
            new Mock<ILogger<BasicTracedConsumer>>().Object,
            null!);

        // Assert
        _ = act.ShouldThrow<ResourceMissingException>();
    }

    [Fact]
    public void Ctor_MissingAppName_ThrowsException()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockFactory = new Mock<IConnectionFactory>();
        _ = mockFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);

        // Act
        var act = () => new BasicTracedConsumer(
            mockFactory.Object,
            new Mock<ITelemeter>().Object,
            new Mock<ILogger<BasicTracedConsumer>>().Object,
            new Mock<IConfiguration>().Object);

        // Assert
        _ = act.ShouldThrow<DataStateException>();
    }

    [Fact]
    public async Task ConsumeAsync_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out _);

        // Act
        var act = () => sut.ConsumeAsync(null!, null!);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public void OnStarting_WhenCalled_WritesExpectedLogs()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);

        // Act
        sut.FireEvent<MqConsumerBase>(nameof(sut.Starting));

        // Assert
        mocks.MockLogger.VerifyLog(msgCheck: s => s == "Mq consumer starting: " + sut.QueueName);
    }

    [Fact]
    public void OnStarted_WhenCalled_WritesExpectedLogs()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);

        // Act
        sut.FireEvent<MqConsumerBase>(nameof(sut.Started));

        // Assert
        mocks.MockLogger.VerifyLog(msgCheck: s => s == "Mq consumer started: " + sut.QueueName);
    }

    [Fact]
    public void OnStopping_WhenCalled_WritesExpectedLogs()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);

        // Act
        sut.FireEvent<MqConsumerBase>(nameof(sut.Stopping));

        // Assert
        mocks.MockLogger.VerifyLog(msgCheck: s => s == "Mq consumer stopping: " + sut.QueueName);
    }

    [Fact]
    public void OnStopped_WhenCalled_WritesExpectedLogs()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);

        // Act
        sut.FireEvent<MqConsumerBase>(nameof(sut.Stopped));

        // Assert
        mocks.MockLogger.VerifyLog(msgCheck: s => s == "Mq consumer stopped: " + sut.QueueName);
    }

    [Fact]
    public void OnMessageReceived_WhenCalled_WritesExpectedLogs()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);

        // Act
        sut.FireEvent<MqConsumerBase<BasicPayload>>(nameof(sut.MessageReceived), GetArgs());

        // Assert
        mocks.MockLogger.VerifyLog(msgCheck: s => s!.StartsWith("Mq message incoming: " + sut.QueueName));
    }

    [Fact]
    public void OnMessageReceived_WhenCalled_TracesActivity()
    {
        // Arrange
        const string expectedName = "mq_consume";
        var sut = GetSut<BasicTracedConsumer>(out var mocks);
        var args = GetArgs();
        var tags = new KeyValuePair<string, object?>[]
        {
            new("queue", sut.QueueName),
            new("born", args.BornOn),
            new("json", args.Message),
            new("attempt", args.AttemptNumber),
        };

        // Act
        sut.FireEvent<MqConsumerBase<BasicPayload>>(nameof(sut.MessageReceived), GetArgs());

        // Assert
        mocks.MockTelemeter.Verify(
            m => m.StartTrace(expectedName, ActivityKind.Consumer, It.IsAny<ActivityContext>(), tags));
    }

    [Fact]
    public void OnMessageProcessed_WhenCalled_WritesExpectedLogs()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);

        // Act
        sut.FireEvent<MqConsumerBase<BasicPayload>>(nameof(sut.MessageProcessed), GetArgs());

        // Assert
        mocks.MockLogger.VerifyLog(msgCheck: s => s!.StartsWith("Mq message success: " + sut.QueueName + "#"));
    }

    [Fact]
    public void OnMessageProcessed_WhenCalled_CapturesMetric()
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);
        const string expectedName = "mq_consume_success";
        var tag = new KeyValuePair<string, object?>[]
        {
            new("queue", sut.QueueName),
            new("outcome", "success"),
        };

        // Act
        sut.FireEvent<MqConsumerBase<BasicPayload>>(nameof(sut.MessageProcessed), GetArgs());

        // Assert
        mocks.MockTelemeter.Verify(
            m => m.CaptureMetric(MetricType.Counter, 1, expectedName, null, null, null, tag));
    }

    [Theory]
    [InlineData(true, "transient", LogLevel.Warning)]
    [InlineData(false, "permanent", LogLevel.Error)]
    public void OnMessageFailed_VaryingRetry_WritesExpectedLogs(bool retry, string name, LogLevel logLevel)
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);
        var args = GetErrorArgs(retry);

        // Act
        sut.FireEvent<MqConsumerBase<BasicPayload>>(nameof(sut.MessageFailed), args);

        // Assert
        mocks.MockLogger.VerifyLog(logLevel, s => s!.StartsWith($"Mq {name} error: {sut.QueueName}#"));
    }

    [Theory]
    [InlineData(true, "transient")]
    [InlineData(false, "permanent")]
    public void OnMessageFailed_VaryingRetry_CapturesMetric(bool retry, string outcome)
    {
        // Arrange
        var sut = GetSut<BasicTracedConsumer>(out var mocks);
        var args = GetErrorArgs(retry);
        const string expectedName = "mq_consume_failure";
        var tag = new KeyValuePair<string, object?>[]
        {
            new("queue", sut.QueueName),
            new("outcome", outcome + " error"),
        };

        // Act
        sut.FireEvent<MqConsumerBase<BasicPayload>>(nameof(sut.MessageFailed), args);

        // Assert
        mocks.MockTelemeter.Verify(
            m => m.CaptureMetric(MetricType.Counter, 1, expectedName, null, null, null, tag));
    }

    private static MqConsumerEventArgs GetArgs(
        int attempt = 1,
        long born = 10000000,
        ulong deliveryId = 1,
        string message = "{ \"hello\": \"world\" }",
        Guid guid = default) => new()
        {
            AttemptNumber = attempt,
            BornOn = born,
            DeliveryId = deliveryId,
            Message = message,
            MessageGuid = guid,
        };

    private static MqFailedEventArgs GetErrorArgs(
        bool? retry = null,
        Exception? ex = null,
        int attempt = 1,
        long born = 10000000,
        ulong deliveryId = 1,
        string message = "{ \"hello\": \"world\" }",
        Guid guid = default) => new()
        {
            Retry = retry,
            Error = ex ?? new ArithmeticException("mathz fail"),
            AttemptNumber = attempt,
            BornOn = born,
            DeliveryId = deliveryId,
            Message = message,
            MessageGuid = guid,
        };

    private static T GetSut<T>(
        out BagOfMocks<T> mocks,
        string appName = "test",
        int? maxAttemptsQConfig = 5,
        int? maxAttemptsXConfig = null)
        where T : MqConsumerBase
    {
        var queue = $"{appName}-{TestHelper.TestExchangeName}";
        var configDicto = new Dictionary<string, string?>()
        {
            ["RabbitMq:ConsumerAppName"] = appName,
            [$"RabbitMq:Queues:{queue}:MaximumAttempts"] = $"{maxAttemptsQConfig}",
            [$"RabbitMq:Exchanges:{TestHelper.TestExchangeName}:MaximumAttempts"] = $"{maxAttemptsXConfig}",
        };

        var memConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(configDicto)
            .Build();

        mocks = new(
            new Mock<IModel>(),
            new Mock<ITelemeter>(),
            new Mock<ILogger<T>>());

        var mockProps = new Mock<IBasicProperties>();
        _ = mocks.MockChannel
            .Setup(m => m.CreateBasicProperties())
            .Returns(mockProps.Object);

        var mockConnection = new Mock<IConnection>();
        _ = mockConnection
            .Setup(m => m.CreateModel())
            .Returns(mocks.MockChannel.Object);

        var mockConnectionFactory = new Mock<IConnectionFactory>();
        _ = mockConnectionFactory
            .Setup(m => m.CreateConnection())
            .Returns(mockConnection.Object);

        return (T)Activator.CreateInstance(
            typeof(T),
            mockConnectionFactory.Object,
            mocks.MockTelemeter.Object,
            mocks.MockLogger.Object,
            memConfig)!;
    }

    private sealed record BagOfMocks<T>(
        Mock<IModel> MockChannel,
        Mock<ITelemeter> MockTelemeter,
        Mock<ILogger<T>> MockLogger);
}
