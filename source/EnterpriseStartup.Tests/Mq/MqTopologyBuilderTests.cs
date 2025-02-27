// <copyright file="MqTopologyBuilderTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using EnterpriseStartup.Messaging;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging.Abstractions.Producer;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseStartup.Tests.Mq;

/// <summary>
/// Tests for the <see cref="MqTopologyBuilder"/> class.
/// </summary>
public class MqTopologyBuilderTests
{
    [Fact]
    public void AddMqConsumer_WhenCalled_RegistersExpected()
    {
        // Arrange
        var mockServices = new Mock<IServiceCollection>();
        var sut = new MqTopologyBuilder(mockServices.Object);

        // Act
        sut.AddMqConsumer<FakeConsumer>();

        // Assert
        mockServices.Verify(
            m => m.Add(
                It.Is<ServiceDescriptor>(
                    s => s.ServiceType == typeof(FakeConsumer))));
    }

    [Fact]
    public void AddMqProducer_WhenCalled_RegistersExpected()
    {
        // Arrange
        var mockServices = new Mock<IServiceCollection>();
        var sut = new MqTopologyBuilder(mockServices.Object);

        // Act
        sut.AddMqProducer<FakeProducer>();

        // Assert
        mockServices.Verify(
            m => m.Add(
                It.Is<ServiceDescriptor>(
                    s => s.ServiceType == typeof(FakeProducer))));
    }
}

[ExcludeFromCodeCoverage]
public record FakeMessage(string Id);

[ExcludeFromCodeCoverage]
public class FakeConsumer : MqConsumerBase<FakeMessage>
{
    public override string ExchangeName => "Exchange";

    public override bool IsConnected => true;

    public override Task ConsumeAsync(FakeMessage message, MqConsumerEventArgs args)
        => throw new NotImplementedException();

    protected override Task StartInternal(CancellationToken token)
        => throw new NotImplementedException();

    protected override Task StopInternal(CancellationToken token)
        => throw new NotImplementedException();
}

[ExcludeFromCodeCoverage]
public class FakeProducer : MqProducerBase<FakeMessage>
{
    public override string ExchangeName => "Exchange";

    public override bool IsConnected => true;

    protected override Guid ProduceInternal(byte[] bytes, Dictionary<string, object> headers)
        => throw new NotImplementedException();
}