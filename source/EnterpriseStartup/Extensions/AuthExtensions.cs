// <copyright file="AuthExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

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
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(
                opts =>
                {
                    configuration.Bind(configSection, opts);
                    opts.TokenValidationParameters.NameClaimType = "name";
                },
                opts => configuration.Bind(configSection, opts));

        return services;
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
        app.UseAuthentication();
        app.MapControllers().RequireAuthorization();

        return app;
    }
}
