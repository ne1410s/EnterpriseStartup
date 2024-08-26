// <copyright file="UserExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Auth;

using System.Security.Claims;
using EnterpriseStartup.Auth;
using FluentErrors.Errors;

/// <summary>
/// Tests for the <see cref="UserExtensions"/> class.
/// </summary>
public class UserExtensionsTests
{
    [Fact]
    public void ToUser_NoAuth_ThrowsExpected()
    {
        // Arrange
        var principal = new ClaimsPrincipal([]);

        // Act
        var act = principal.ToUser;

        // Assert
        act.Should().Throw<DataStateException>();
    }

    [Fact]
    public void ToUser_AuthedButNoSubject_ThrowsExpected()
    {
        // Arrange
        var principal = new ClaimsPrincipal([new([], "fake")]);

        // Act
        var act = principal.ToUser;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
