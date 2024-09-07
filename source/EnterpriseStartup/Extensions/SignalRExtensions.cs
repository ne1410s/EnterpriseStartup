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

/// <summary>
/// Extensions relating to SignalR.
/// </summary>
public static class SignalRExtensions
{
    private const string HubPath = "/notifications-hub";

    /// <summary>
    /// Adds the enterprise SignalR feature. This registers SignalR for startup health checks.
    /// This makes an <see cref="INotifier"/> available to DI, for sending server-to-client notices.
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
        services.AddSignalR();
        services.AddScoped<INotifier, SignalRNotifier>()
            .AddHealthChecks()
            .AddSignalRHub(hubUri, tags: [HealthExtensions.NonVital]);

        return services;
    }

    /// <summary>
    /// Uses the enterprise SignalR feature.
    /// </summary>
    /// <typeparam name="T">The app builder type.</typeparam>
    /// <param name="app">The application builder.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IApplicationBuilder UseEnterpriseSignalR<T>(this T app)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        app.MapHub<NotificationsHub>(HubPath);
        return app;
    }
}
