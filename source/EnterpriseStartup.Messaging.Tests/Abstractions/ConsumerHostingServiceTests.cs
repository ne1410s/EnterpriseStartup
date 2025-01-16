// <copyright file="ConsumerHostingServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Tests.Abstractions;

using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="ConsumerHostingService{TConsumer}"/> class.
/// </summary>
public class ConsumerHostingServiceTests
{
    [Fact]
    public void Ctor_FailedConsumer_ThrowsException()
    {
        // Arrange & Act
        var act = () => GetBasicSut(out _, out _, false);

        // Assert
        _ = act.ShouldThrow<Exception>();
    }

    [Fact]
    public async Task StartAsync_FromCtor_LogsExpected()
    {
        // Arrange
        using var sut = GetBasicSut(out var consumer, out var mockLogger);

        // Act
        var act = async () =>
        {
            await sut.StartAsync(CancellationToken.None);
            await sut.StopAsync(CancellationToken.None);
        };

        // Assert
        await act.ShouldNotThrowAsync();
        consumer.Lifecycle.ShouldContain("StartInternal");
        mockLogger.VerifyLog(LogLevel.Information, s => s == "Starting up...");
        mockLogger.VerifyLog(LogLevel.Information, s => s == "Started ok!");
    }

    [Fact]
    public async Task ExecuteAsync_NoConnection_ThrowsException()
    {
        // Arrange
        using var sut = GetBasicSut(out var mockConsumer, out var mockLogger, true);
        mockConsumer.Starting += (_, _) => throw new ArithmeticException("mathzz");
        using var cts = new CancellationTokenSource(200);

        // Act
        var act = () => sut.StartAsync(cts.Token);

        // Assert
        await act.ShouldNotThrowAsync();
        mockLogger.VerifyLog(LogLevel.Warning, s => s!.StartsWith("Failed to start"));
    }

    [Fact]
    public async Task ExecuteAsync_InstantCancel_DoesNotThrow()
    {
        // Arrange
        using var sut = GetBasicSut(out _, out _, true);
        using var cts = new CancellationTokenSource(1);
        await Task.Delay(50);

        // Act
        var act = () => sut.StartAsync(cts.Token);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    private static ConsumerHostingService<GenericConsumer> GetBasicSut(
        out GenericConsumer consumer,
        out Mock<ILogger> mockLogger,
        bool resolveConsumerOk = true)
    {
        var mockChannel = new Mock<IModel>();
        var mockConnection = new Mock<IConnection>();
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        _ = mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);
        _ = mockConnectionFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);
        consumer = new GenericConsumer(0);
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockProvider = new Mock<IServiceProvider>();
        mockLogger = new Mock<ILogger>();
        var mockLogFactory = new Mock<ILoggerFactory>();
        var consumerType = consumer.GetType();
        _ = mockProvider.Setup(m => m.GetService(consumerType)).Returns(consumer);
        if (!resolveConsumerOk)
        {
            _ = mockProvider.Setup(m => m.GetService(consumerType)).Throws(new ArithmeticException("mathzz"));
        }

        _ = mockProvider.Setup(m => m.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);
        _ = mockScope.Setup(m => m.ServiceProvider).Returns(mockProvider.Object);
        _ = mockScopeFactory.Setup(m => m.CreateScope()).Returns(mockScope.Object);
        var loggerCategory = $"ConsumerHostingService<{consumerType.Name}>";
        _ = mockLogFactory.Setup(m => m.CreateLogger(loggerCategory)).Returns(mockLogger.Object);
        return new ConsumerHostingService<GenericConsumer>(mockProvider.Object, mockLogFactory.Object);
    }
}