// <copyright file="NotificationsHubInMemTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.SignalR;

using System.Security.Claims;
using EnterpriseStartup.SignalR;

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

    [Fact]
    public async Task OnConnected_AuthenticUser_AddsUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        using var sut = new NotificationsHubInMem();
        sut.Context = new FakeContext(GetUser(userId));

        // Act
        await sut.OnConnectedAsync();

        // Assert
        NotificationsHubInMem.ConnectedUsers.Keys.ShouldContain(userId);
    }

    [Fact]
    public async Task OnConnected_UserAlreadyConnected_AddsConnection()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        NotificationsHubInMem.ConnectedUsers[userId] = ["connection0"];
        using var sut = new NotificationsHubInMem();
        sut.Context = new FakeContext(GetUser(userId));

        // Act
        await sut.OnConnectedAsync();

        // Assert
        NotificationsHubInMem.ConnectedUsers[userId].Count.ShouldBe(2);
    }

    [Fact]
    public async Task OnDisconnected_AuthenticUser_RemovesUserConnection()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        const string connectionId = "connection1";
        NotificationsHubInMem.ConnectedUsers[userId] = [connectionId];
        using var sut = new NotificationsHubInMem();
        sut.Context = new FakeContext(GetUser(userId), connectionId);

        // Act
        await sut.OnDisconnectedAsync(null);

        // Assert
        NotificationsHubInMem.ConnectedUsers.Keys.ShouldNotContain(userId);
    }

    private static ClaimsPrincipal GetUser(string id)
        => new([new([new(ClaimTypes.NameIdentifier, id)], "fake")]);
}