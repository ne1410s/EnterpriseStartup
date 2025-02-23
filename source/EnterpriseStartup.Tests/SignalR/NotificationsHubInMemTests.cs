// <copyright file="NotificationsHubInMemTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.SignalR;

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using EnterpriseStartup.SignalR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Tests for the <see cref="NotificationsHubInMem"/> class.
/// </summary>
public class NotificationsHubInMemTests
{
    [Fact]
    public async Task OnConnected_NoUser_Aborts()
    {
        // Arrange
        var fakeContext = new FakeContext(null);
        using var sut = new NotificationsHubInMem();
        sut.Context = fakeContext;
        _ = fakeContext.UserIdentifier;
        _ = fakeContext.ConnectionAborted;

        // Act
        await sut.OnConnectedAsync();

        // Assert
        fakeContext.Aborts.ShouldBe(1);
    }

    [Fact]
    public async Task OnConnected_InauthenticUser_Aborts()
    {
        // Arrange
        var user = new ClaimsPrincipal();
        var fakeContext = new FakeContext(user);
        using var sut = new NotificationsHubInMem();
        sut.Context = fakeContext;

        // Act
        await sut.OnConnectedAsync();

        // Assert
        fakeContext.Aborts.ShouldBe(1);
    }
}

public class FakeContext(ClaimsPrincipal? user, string connectionId = "connection") : HubCallerContext
{
    public int Aborts { get; private set; }

    public override string ConnectionId => connectionId;

    public override string? UserIdentifier { get; }

    public override ClaimsPrincipal? User => user;

    public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

    public override IFeatureCollection Features { get; } = new FeatureCollection();

    public override CancellationToken ConnectionAborted { get; }

    public override void Abort() => this.Aborts++;
}
