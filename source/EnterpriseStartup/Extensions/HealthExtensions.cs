// <copyright file="HealthExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System.IO;
using System.Linq;
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
