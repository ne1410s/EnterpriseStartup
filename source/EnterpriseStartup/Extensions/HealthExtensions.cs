// <copyright file="HealthExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Extensions relating to health / health checks.
/// </summary>
public static class HealthExtensions
{
    /// <summary>
    /// A health check tag that can be used to omit the dependency from readiness checks.
    /// </summary>
    public const string NonVital = "non-vital";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    static HealthExtensions()
    {
        JsonOpts.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Adds the enterprise health feature.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The health checks builder, for chaining additional checks.</returns>
    public static IHealthChecksBuilder AddEnterpriseHealthChecks(
        this IServiceCollection services)
    {
        return services.AddHealthChecks();
    }

    /// <summary>
    /// Adds a HTTP healthcheck. Generally, a simple "ping" endpoint to infer
    /// the liveness of the dependency should suffice. If you absolutely require
    /// the HTTP dependency to function then you may wish to conduct a more
    /// thorough test, such as a readiness probe where available, or else a
    /// functional query similar to how your app would call it. (Just bear in
    /// mind this needs to be called frequently, so should be something fast and
    /// entirely non-destructive!.
    /// </summary>
    /// <param name="builder">The health check builder.</param>
    /// <param name="probeUri">The uri to call.</param>
    /// <param name="checkName">The name of the healthcheck.</param>
    /// <param name="vital">Whether this is crucial to your application.</param>
    /// <param name="tags">Specific tags on the healthcheck.</param>
    /// <param name="configureClient">Optional http client configurer.</param>
    /// <returns>The health check builder, for chainable calls.</returns>
    public static IHealthChecksBuilder AddHttp(
        this IHealthChecksBuilder builder,
        Uri probeUri,
        string checkName,
        bool vital = true,
        IEnumerable<string>? tags = null,
        Action<IServiceProvider, HttpClient>? configureClient = null)
    {
        tags = vital ? [.. tags ?? []] : [NonVital, .. tags ?? []];
        return builder.AddUrlGroup(probeUri, checkName, tags: tags, configureClient: configureClient);
    }

    /// <summary>
    /// Uses the enterprise health feature.
    /// </summary>
    /// <typeparam name="T">The app builder type.</typeparam>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static T UseEnterpriseHealthChecks<T>(this T app)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        // Start: include all dependencies
        app.MapHealthChecks("healthz", new() { ResponseWriter = WriteResponse });

        // Ready: exclude non-vital dependencies
        app.MapHealthChecks(
            "healthz/ready",
            new() { Predicate = c => !c.Tags.Contains("non-vital"), ResponseWriter = WriteResponse });

        // Live: exclude all dependencies
        app.MapHealthChecks("healthz/live", new() { Predicate = _ => false });

        return app;
    }

    private static Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var report = new
        {
            healthReport.Status,
            Deps = healthReport.Entries.Select(r => new
            {
                r.Key,
                r.Value.Status,
                r.Value.Description,
                r.Value.Duration,
                r.Value.Data,
            }),
        };

        var json = JsonSerializer.Serialize(report, JsonOpts);
        return context.Response.WriteAsync(json);
    }
}
