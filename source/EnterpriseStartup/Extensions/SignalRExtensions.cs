// <copyright file="SignalRExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using EnterpriseStartup.SignalR;
using FluentErrors.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static StackExchange.Redis.RedisChannel;

/// <summary>
/// Extensions relating to SignalR.
/// </summary>
public static class SignalRExtensions
{
    private const string HubPath = "/notifications-hub";

    /// <summary>
    /// Adds the enterprise SignalR feature. This registers SignalR for startup health checks.
    /// This makes an <see cref="INotifier"/> available to DI, for sending server-to-client notices.
    /// If a "Redis" connection string is found, it will be used as a backplane for SignalR.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration. For the health check to function, a config
    /// entry under "HostedBaseUrl" must be set to the path under which the api is hosted.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseSignalR(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        configuration.MustExist();
        var hubUri = $"{configuration["HostedBaseUrl"]?.Trim('/')}/{HubPath.Trim('/')}";
        var redis = configuration.GetConnectionString("Redis");
        if (redis != null)
        {
            _ = services
                .AddScoped<INotifier, SignalRNotifier>()
                .AddSignalR()
                .AddStackExchangeRedis(redis, opts =>
                    opts.Configuration.ChannelPrefix = new("SignalR", PatternMode.Literal));
        }
        else
        {
            _ = services
                .AddScoped<INotifier, SignalRInMemNotifier>()
                .AddSignalR();
        }

        _ = services
            .AddHealthChecks()
            .AddSignalRHub(hubUri, tags: [HealthExtensions.NonVital]);

        return services;
    }

    /// <summary>
    /// Uses the enterprise SignalR feature.
    /// </summary>
    /// <typeparam name="T">The app builder type.</typeparam>
    /// <param name="app">The application builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IApplicationBuilder UseEnterpriseSignalR<T>(this T app, IConfiguration configuration)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        var isRedis = !string.IsNullOrEmpty(configuration.GetConnectionString("Redis"));
        if (isRedis)
        {
            app.MapHub<NotificationsHub>(HubPath);
        }
        else
        {
            app.MapHub<NotificationsHubInMem>(HubPath);
        }

        return app;
    }
}
