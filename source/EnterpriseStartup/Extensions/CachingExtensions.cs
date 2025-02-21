// <copyright file="CachingExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System;
using EnterpriseStartup.Caching;
using EnterpriseStartup.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

/// <summary>
/// Extensions relating to caching.
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Adds enterprise caching. This requires a Redis instance.
    /// This makes an <see cref="INotifier"/> available to DI, for sending server-to-client notices.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration. For the health check to function, a config
    /// entry under "HostedBaseUrl" must be set to the path under which the api is hosted.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        var connection = configuration.GetConnectionString("Redis");
        if (connection != null)
        {
            _ = services
                .AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connection))
                .AddScoped(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase())
                .AddTransient<ICache, RedisCache>()
                .AddHealthChecks()
                .AddRedis(connection, tags: ["non-vital"]);
        }
        else
        {
            services.AddSingleton<ICache, MemoryCache>();
        }

        return services;
    }
}