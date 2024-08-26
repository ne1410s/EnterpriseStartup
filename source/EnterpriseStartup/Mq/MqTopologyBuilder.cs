// <copyright file="MqTopologyBuilder.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Mq;

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
    /// </summary>
    /// <typeparam name="T">The consumer type.</typeparam>
    /// <returns>The original parameter, for chainable commands.</returns>
    public MqTopologyBuilder AddMqConsumer<T>()
        where T : MqConsumerBase
    {
        services
            .AddScoped<T>()
            .AddHostedService<ConsumerHostingService<T>>();
        return this;
    }

    /// <summary>
    /// Adds a mq producer.
    /// </summary>
    /// <typeparam name="T">The producer type.</typeparam>
    /// <returns>The original parameter, for chainable commands.</returns>
    public MqTopologyBuilder AddMqProducer<T>()
        where T : class, IMqProducer
    {
        services.AddScoped<T>();
        return this;
    }
}
