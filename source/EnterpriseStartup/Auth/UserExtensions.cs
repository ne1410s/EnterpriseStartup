// <copyright file="UserExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using FluentErrors.Extensions;

/// <summary>
/// Extensions for the <see cref="ClaimsPrincipal"/> user.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Gets an enterprise user from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>An enterprise user.</returns>
    public static EnterpriseUser ToUser(this ClaimsPrincipal principal)
    {
        var identity = principal?.Identity;
        identity?.IsAuthenticated.MustBe(true);
        var subject = principal!.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub);
        return new(subject.Value);
    }
}
