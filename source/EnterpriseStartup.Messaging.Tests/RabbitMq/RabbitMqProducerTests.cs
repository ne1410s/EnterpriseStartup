// <copyright file="RabbitMqProducerTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Tests.RabbitMq;

using EnterpriseStartup.Messaging.Abstractions.Producer;
using EnterpriseStartup.Messaging.RabbitMq;
using RabbitMQ.Client;

/// <summary>
/// Tests for the <see cref="RabbitMqProducer{T}"/> class.
/// </summary>
public class RabbitMqProducerTests
{
    [Fact]
    public void Produce_Multiple_DeclaresExchangeOnce()
    {
        // Arrange
        var sut = GetSut<BasicProducer>(out var mocks);
        _ = mocks.MockConnection.Setup(m => m.IsOpen).Returns(true);

        // Act
        sut.Produce(new(null));
        sut.Produce(new(null));

        // Assert
        mocks.MockChannel.Verify(
            m => m.ExchangeDeclare(sut.ExchangeName, ExchangeType.Direct, true, false, null),
            Times.Once());
    }

    [Fact]
    public void Produce_NotConnected_DeclaresExchange()
    {
        // Arrange
        var sut = GetSut<BasicProducer>(out var mocks);

        // Act
        sut.Produce(new(null));

        // Assert
        mocks.MockChannel.Verify(m => m.ExchangeDeclare(sut.ExchangeName, ExchangeType.Direct, true, false, null));
    }

    [Fact]
    public void Produce_WithFactory_DeclaresExchange()
    {
        // Arrange
        var sut = GetSut<BasicProducer>(out var mocks);

        // Act
        sut.Produce(new(null));

        // Assert
        sut.IsConnected.ShouldBeFalse();
        mocks.MockChannel.Verify(
            m => m.ExchangeDeclare(sut.ExchangeName, ExchangeType.Direct, true, false, null));
    }

    [Fact]
    public void Dispose_WhenCalled_ClosesChannelAndConnection()
    {
        // Arrange
        var sut = GetSut<BasicProducer>(out var mocks);
        sut.Produce(new(null));

        // Act
        sut.Dispose();

        // Assert
        mocks.MockChannel.Verify(m => m.Close());
        mocks.MockConnection.Verify(m => m.Close());
    }

    [Fact]
    public void Dispose_NullChannelAndConnection_DoesNotError()
    {
        // Arrange
        var mockFactory = new Mock<IConnectionFactory>();
        var sut = new BasicProducer(mockFactory.Object);

        // Act
        sut.Dispose();

        // Assert!
        1.ShouldBe(1);
    }

    [Fact]
    public void Produce_WhenCalled_CallsBasicPublish()
    {
        // Arrange
        var sut = GetSut<BasicProducer>(out var mocks);

        // Act
        sut.Produce(new(null));

        // Assert
        mocks.MockChannel.Verify(
            m => m.BasicPublish(
                sut.ExchangeName,
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.Is<IBasicProperties>(m => m.Headers.ContainsKey("x-attempt")
                    && m.Headers.ContainsKey("x-born")
                    && m.Headers.ContainsKey("x-guid")),
                It.IsAny<ReadOnlyMemory<byte>>()));
    }

    [Fact]
    public void Produce_WhenCalled_HitsEventHandlers()
    {
        // Arrange
        var sut = GetSut<BasicProducer>(out _);
        var count = 0;
        sut.MessageSending += (_, _) => count++;
        sut.MessageSent += (_, _) => count++;

        // Act
        sut.Produce(new(null));

        // Assert
        count.ShouldBe(2);
    }

    private static T GetSut<T>(
        out BagOfMocks mocks)
        where T : IMqProducer
    {
        mocks = new(
            new Mock<IModel>(),
            new Mock<IConnection>(),
            new Mock<IBasicProperties>());

        _ = mocks.MockProperties
            .Setup(m => m.Headers)
            .Returns(new Dictionary<string, object>());

        _ = mocks.MockChannel
            .Setup(m => m.CreateBasicProperties())
            .Returns(mocks.MockProperties.Object);

        _ = mocks.MockConnection
            .Setup(m => m.CreateModel())
            .Returns(mocks.MockChannel.Object);

        var mockConnectionFactory = new Mock<IConnectionFactory>();
        _ = mockConnectionFactory
            .Setup(m => m.CreateConnection())
            .Returns(mocks.MockConnection.Object);

        return (T)Activator.CreateInstance(
            typeof(T),
            mockConnectionFactory.Object)!;
    }

    private sealed record BagOfMocks(
        Mock<IModel> MockChannel,
        Mock<IConnection> MockConnection,
        Mock<IBasicProperties> MockProperties);
}
