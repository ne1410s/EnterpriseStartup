// <copyright file="DiscoveryExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

/// <summary>
/// Extensions relating to discovery / swagger.
/// </summary>
public static class DiscoveryExtensions
{
    /// <summary>
    /// Adds the enterprise discovery feature.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="requireAuth">Whether to require auth on swagger endpoints.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseDiscovery(
        this IServiceCollection services, bool requireAuth = true)
    {
        return services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(setup =>
            {
                if (requireAuth)
                {
                    var jwtSecurityScheme = new OpenApiSecurityScheme
                    {
                        BearerFormat = "JWT",
                        Name = "JWT Authentication",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
                        Reference = new()
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme,
                        },
                    };

                    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                    setup.AddSecurityRequirement(new() { { jwtSecurityScheme, [] } });
                }
            });
    }

    /// <summary>
    /// Uses the enterprise discovery feature.
    /// </summary>
    /// <param name="app">The app builder.</param>
    /// <param name="environment">The environment.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IApplicationBuilder UseEnterpriseDiscovery(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}
