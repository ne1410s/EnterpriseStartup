// <copyright file="TelemetryExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System.Reflection;
using EnterpriseStartup.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

/// <summary>
/// Extensions relating to telemetry.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds the enterprise telemetry feature.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName();
        var appName = assemblyName.Name!;
        var appVersion = assemblyName.Version?.ToString(3);

        services.AddSingleton<ITelemeter, Telemeter>();
        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder
                .AddTelemetrySdk()
                .AddService(appName, serviceVersion: appVersion)
                .AddEnvironmentVariableDetector())
            .WithTracing(builder => builder
                .AddSource(appName)
                .SetSampler<AlwaysOnSampler>()
                .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new(configuration["OpenTel:Grpc"]!)))
            .WithMetrics(builder => builder
                .AddMeter(appName)
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new(configuration["OpenTel:Grpc"]!)));

        return services;
    }
}
