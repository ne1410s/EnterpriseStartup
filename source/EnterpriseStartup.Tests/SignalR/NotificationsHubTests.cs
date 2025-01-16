// <copyright file="NotificationsHubTests.cs" company="ne1410s">
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
/// Tests for the <see cref="NotificationsHub"/> class.
/// </summary>
public class NotificationsHubTests
{
    [Fact]
    public async Task OnConnected_NoUser_Aborts()
    {
        // Arrange
        var fakeContext = new FakeContext(null);
        using var sut = new NotificationsHub();
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
        using var sut = new NotificationsHub();
        sut.Context = fakeContext;

        // Act
        await sut.OnConnectedAsync();

        // Assert
        fakeContext.Aborts.ShouldBe(1);
    }

    [Fact]
    public async Task OnConnected_AuthenticUser_AddsUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        using var sut = new NotificationsHub();
        sut.Context = new FakeContext(GetUser(userId));

        // Act
        await sut.OnConnectedAsync();

        // Assert
        NotificationsHub.ConnectedUsers.Keys.ShouldContain(userId);
    }

    [Fact]
    public async Task OnConnected_UserAlreadyConnected_AddsConnection()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        NotificationsHub.ConnectedUsers[userId] = ["connection0"];
        using var sut = new NotificationsHub();
        sut.Context = new FakeContext(GetUser(userId));

        // Act
        await sut.OnConnectedAsync();

        // Assert
        NotificationsHub.ConnectedUsers[userId].Count.ShouldBe(2);
    }

    [Fact]
    public async Task OnDisconnected_AuthenticUser_RemovesUserConnection()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        const string connectionId = "connection1";
        NotificationsHub.ConnectedUsers[userId] = [connectionId];
        using var sut = new NotificationsHub();
        sut.Context = new FakeContext(GetUser(userId), connectionId);

        // Act
        await sut.OnDisconnectedAsync(null);

        // Assert
        NotificationsHub.ConnectedUsers.Keys.ShouldNotContain(userId);
    }

    private static ClaimsPrincipal GetUser(string id)
        => new([new([new(ClaimTypes.NameIdentifier, id)], "fake")]);
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
