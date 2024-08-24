// <copyright file="ConsumerHostingServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.tests.Abstractions;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

/// <summary>
/// Tests for the <see cref="ConsumerHostingService{TConsumer}"/> class.
/// </summary>
public class ConsumerHostingServiceTests
{
    [Fact]
    public async Task StartAsync_FromCtor_DoesNotThrow()
    {
        // Arrange
        var mockChannel = new Mock<IModel>();
        var mockConnection = new Mock<IConnection>();
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);
        mockConnectionFactory.Setup(m => m.CreateConnection()).Returns(mockConnection.Object);
        var consumer = new BasicConsumer(mockConnectionFactory.Object);
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockProvider = new Mock<IServiceProvider>();
        var mockLogFactory = new Mock<ILoggerFactory>();
        mockProvider.Setup(m => m.GetService(consumer.GetType())).Returns(consumer);
        mockProvider.Setup(m => m.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);
        mockScope.Setup(m => m.ServiceProvider).Returns(mockProvider.Object);
        mockScopeFactory.Setup(m => m.CreateScope()).Returns(mockScope.Object);
        using var sut = new ConsumerHostingService<BasicConsumer>(mockProvider.Object, mockLogFactory.Object);

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
}