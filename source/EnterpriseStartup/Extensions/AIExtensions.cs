// <copyright file="AIExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System;
using EnterpriseStartup.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Extensions relating to AI.
/// </summary>
public static class AIExtensions
{
    /// <summary>
    /// Adds enterprise AI. This requires a Open AI account key.
    /// This method makes an <see cref="IAIChatCompleter"/> instance available
    /// to DI that can be resolved from that service type.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="sectionName">The name of the configuration section that
    /// specifies the <see cref="OpenAIClientConfig"/>.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseAI(
        this IServiceCollection services,
        string sectionName = nameof(OpenAIClient))
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var aiConfig = new OpenAIClientConfig();
            configuration.Bind(sectionName, aiConfig);
            return Options.Create(aiConfig);
        });

        services.AddScoped<IAIChatCompleter, OpenAIClient>();
        services.AddKeyedScoped<IAIChatCompleter, OpenAIClient>(typeof(OpenAIClient).FullName);
        services.AddHttpClient(nameof(OpenAIClient), (sp, client) =>
        {
            var clientConfig = sp.GetRequiredService<IOptions<OpenAIClientConfig>>().Value;
            client.Timeout = TimeSpan.FromSeconds(clientConfig.HttpTimeoutSeconds);
            client.BaseAddress = new(clientConfig.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new("Bearer", clientConfig.ApiKey);
        }).WithBackoff();

        return services;
    }
}