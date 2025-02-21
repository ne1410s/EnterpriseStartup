// <copyright file="MqTopologyBuilder.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Mq;

using System;
using System.Linq;
using EnterpriseStartup.Messaging.Abstractions.Consumer;
using EnterpriseStartup.Messaging.Abstractions.Producer;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Mq topology builder.
/// </summary>
public class MqTopologyBuilder(IServiceCollection services)
{
    /// <summary>
    /// Adds a mq consumer service.
    /// You can resolve the consumer as either:
    /// <list type="bullet">
    ///   <item><i>IMqConsumer&lt;TMessage&gt;</i> (by interface - <b>recommended</b>)</item>
    ///   <item><i>provider.GetKeyedService&lt;IMqConsumer&lt;TMessage&gt;&gt;(nameof(TConsumer))</i> (by key, e.g. multiple consumers of the same message type)</item>
    ///   <item><i>TConsumer</i> (by concrete type)</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TConsumer">The consumer type.</typeparam>
    /// <returns>The original parameter, for chainable commands.</returns>
    public MqTopologyBuilder AddMqConsumer<TConsumer>()
        where TConsumer : MqConsumerBase
    {
        var implementationType = typeof(TConsumer);
        var serviceType = GetConsumerServiceType<TConsumer>();
        _ = services
            .AddTransient(implementationType)
            .AddTransient(serviceType, implementationType)
            .AddKeyedTransient(serviceType, typeof(TConsumer).FullName, implementationType)
            .AddHostedService<ConsumerHostingService<TConsumer>>();

        return this;
    }

    /// <summary>
    /// Adds a mq producer.
    /// You can resolve the producer as either:
    /// <list type="bullet">
    ///   <item><i>IMqProducer&lt;TMessage&gt;</i> (by interface - <b>recommended</b>)</item>
    ///   <item><i>provider.GetKeyedService&lt;IMqProducer&lt;TMessage&gt;&gt;(nameof(TProducer))</i> (by key, e.g. multiple producers of the same message type)</item>
    ///   <item><i>TProducer</i> (by concrete type)</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TProducer">The producer type.</typeparam>
    /// <returns>The original parameter, for chainable commands.</returns>
    public MqTopologyBuilder AddMqProducer<TProducer>()
        where TProducer : class, IMqProducer
    {
        var implementationType = typeof(TProducer);
        var serviceType = GetProducerServiceType<TProducer>();
        _ = services
            .AddTransient(implementationType)
            .AddTransient(serviceType, implementationType)
            .AddKeyedTransient(serviceType, typeof(TProducer).FullName, implementationType);

        return this;
    }

    private static Type GetConsumerServiceType<TConsumer>()
        where TConsumer : IMqConsumer
    {
        var messageType = typeof(TConsumer)
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMqConsumer<>))
            ?.GetGenericArguments()?.FirstOrDefault()
                ?? throw new InvalidOperationException("Consumer must implement IMqConsumer<T>");

        return typeof(IMqConsumer<>).MakeGenericType(messageType);
    }

    private static Type GetProducerServiceType<TProducer>()
        where TProducer : IMqProducer
    {
        var messageType = typeof(TProducer)
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMqProducer<>))
            ?.GetGenericArguments()?.FirstOrDefault()
                ?? throw new InvalidOperationException("Producer must implement IMqProducer<T>");

        return typeof(IMqProducer<>).MakeGenericType(messageType);
    }
}
