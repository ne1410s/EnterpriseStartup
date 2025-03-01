// <copyright file="AuthExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentErrors.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Extensions relating to authentication and authorisation.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Adds the enterprise Azure Active Directory B2C feature.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configSection">The name of the section where Azure AD B2C
    /// configuration is provided, if different from the default.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseB2C(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "AzureAdB2C")
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(
                opts =>
                {
                    opts.TokenValidationParameters.NameClaimType = ClaimTypes.NameIdentifier;
                    opts.TokenValidationParameters.RoleClaimType = "http://schemas.microsoft.com/identity/claims/scope";
                    opts.Events = new();
                    opts.Events.OnMessageReceived += (MessageReceivedContext ctx) =>
                    {
                        ctx.Token = ctx.Request.Query.TryGetValue("access_token", out var t) ? (string?)t : null;
                        return Task.CompletedTask;
                    };
                },
                identityOpts => configuration.Bind(configSection, identityOpts));

        return services;
    }

    /// <summary>
    /// Adds enterprise JWT. This provides an authorization policy, for validation.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configSection">The name of the section where Azure AD B2C
    /// configuration is provided, if different from the default.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static IServiceCollection AddEnterpriseJWT(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "JWTAuth")
    {
        var section = configuration.MustExist().GetSection(configSection);
        var key = new SymmetricSecurityKey(Convert.FromBase64String(section["IssuerKey"]!));
        var policyName = section["PolicyName"] ?? "EnterpriseJWT";
        var schemeName = $"{policyName}_Scheme";

        _ = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(schemeName, o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = section["Issuer"],
                ValidAudience = section["Audience"],
                IssuerSigningKey = key,
            };
        });

        return services.AddAuthorization(opts =>
        {
            opts.AddPolicy(policyName, builder => builder
                .AddAuthenticationSchemes(schemeName)
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.NameIdentifier));
        });
    }

    /// <summary>
    /// Uses the Azure Active Directory B2C feature by requiring all endpoints
    /// that do not specifically allow anonymous traffic to be authorised.
    /// </summary>
    /// <typeparam name="T">The app builder type.</typeparam>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static T UseEnterpriseB2C<T>(this T app)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        app.UseAuthentication().UseAuthorization();
        app.MapControllers().RequireAuthorization();

        return app;
    }

    /// <summary>
    /// Uses the enterprise JWT feature by requiring all endpoints
    /// that do not specifically allow anonymous traffic to be authorised.
    /// </summary>
    /// <typeparam name="T">The app builder type.</typeparam>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The original parameter, for chainable commands.</returns>
    public static T UseEnterpriseJWT<T>(this T app)
        where T : IApplicationBuilder, IEndpointRouteBuilder
        => app.UseEnterpriseB2C(); // as above!
}
