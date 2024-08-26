// <copyright file="ConsumerHostingServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.tests.Abstractions;

using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging.tests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="ConsumerHostingService{TConsumer}"/> class.
/// </summary>
public class ConsumerHostingServiceTests
{
    [Fact]
    public void Ctor_CannotResolveConsumer_LogsError()
    {
        // Arrange & Act
        using var sut = GetBasicSut(out var consumer, out var mockLogger, false);

        // Assert
        mockLogger.VerifyLog(LogLevel.Error, s => s == "Failed to start consumer hosting service");
        consumer.Dispose();
    }

    [Fact]
    public async Task StartAsync_FromCtor_DoesNotThrow()
    {
        // Arrange
        using var sut = GetBasicSut(out var consumer, out _);

        // Act
        var act = async () =>
        {
            await sut.StartAsync(CancellationToken.None);
            await sut.StopAsync(CancellationToken.None);
        };

        // Assert
        await act.Should().NotThrowAsync();
        consumer.Dispose();
    }

    private static ConsumerHostingService<BasicConsumer> GetBasicSut(
        out BasicConsumer consumer,
        out Mock<ILogger> mockLogger,
        bool resolveConsumerOk = true)
    {
        var mockChannel = new Mock<IModel>();
        var mockConnection = new Mock<IConnection>();
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);
        mockConnectionFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);
        consumer = new BasicConsumer(mockConnectionFactory.Object);
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockProvider = new Mock<IServiceProvider>();
        mockLogger = new Mock<ILogger>();
        var mockLogFactory = new Mock<ILoggerFactory>();
        var consumerType = consumer.GetType();
        mockProvider.Setup(m => m.GetService(consumerType)).Returns(consumer);
        if (!resolveConsumerOk)
        {
            mockProvider.Setup(m => m.GetService(consumerType)).Throws(new ArithmeticException("mathzz"));
        }

        mockProvider.Setup(m => m.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);
        mockScope.Setup(m => m.ServiceProvider).Returns(mockProvider.Object);
        mockScopeFactory.Setup(m => m.CreateScope()).Returns(mockScope.Object);
        mockLogFactory.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        return new ConsumerHostingService<BasicConsumer>(mockProvider.Object, mockLogFactory.Object);
    }
}