// <copyright file="EnterpriseUserTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Auth;

using EnterpriseStartup.Auth;

/// <summary>
/// Tests for the <see cref="EnterpriseUser"/> class.
/// </summary>
public class EnterpriseUserTests
{
    [Fact]
    public void Ctor_WhenNewed_RetainsProperty()
    {
        // Arrange
        const string id = "id";

        // Act
        var user = new EnterpriseUser(id);

        // Assert
        user.Id.Should().Be(id);
    }
}
