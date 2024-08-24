// <copyright file="MqExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

/// <summary>
/// Extensions relating to message queue.
/// </summary>
public static class MqExtensions
{
    /// <summary>
    /// Adds the enterprise mq feature. This registers mq for startup and readiness health checks.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static MqTopologyBuilder AddEnterpriseMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mqSection = configuration.GetRequiredSection("RabbitMq");
        var factory = new ConnectionFactory
        {
            UserName = mqSection["Username"],
            Password = mqSection["Password"],
            HostName = mqSection["Hostname"],
            Port = mqSection.GetValue<int?>("Port") ?? AmqpTcpEndpoint.UseDefaultPort,
            DispatchConsumersAsync = true,
        };

        services.AddSingleton<IConnectionFactory>(_ => factory)
            .AddHealthChecks()
            .AddRabbitMQ();

        return new MqTopologyBuilder(services);
    }
}
