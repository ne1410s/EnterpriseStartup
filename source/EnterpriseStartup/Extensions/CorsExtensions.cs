﻿// <copyright file="CorsExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions relating to cors.
/// </summary>
public static class CorsExtensions
{
    private static readonly string[] SignalRHeaders =
    [
        "x-requested-with",
        "x-signalr-user-agent",
    ];

    /// <summary>
    /// Adds the enterprise cors feature.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="origins">The allowed origins.</param>
    /// <param name="headers">The allowed headers.</param>
    /// <param name="exposedHeaders">The headers allowed to be exposed in responses.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseCors(
        this IServiceCollection services,
        string[]? origins = null,
        string[]? headers = null,
        string[]? exposedHeaders = null)
    {
        headers = ["Authorization", "Content-Type", .. SignalRHeaders, .. headers ?? []];
        exposedHeaders = ["Content-Disposition", .. exposedHeaders ?? []];
        return services.AddCors(o => o
            .AddPolicy(nameof(EnterpriseStartup), builder => builder
                .AllowCredentials()
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                .WithHeaders(headers)
                .WithExposedHeaders(exposedHeaders)
                .WithOrigins(origins?.Length > 0 ? origins : ["*"])
                .SetIsOriginAllowedToAllowWildcardSubdomains()));
    }

    /// <summary>
    /// Uses the enterprise cors feature.
    /// </summary>
    /// <param name="app">The app builder.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IApplicationBuilder UseEnterpriseCors(
        this IApplicationBuilder app)
    {
        return app.UseCors(nameof(EnterpriseStartup));
    }
}
