// <copyright file="NotificationsHubTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.SignalR;

using System.Security.Claims;
using EnterpriseStartup.SignalR;
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
        sut.Groups = new Mock<IGroupManager>().Object;
        sut.Context = fakeContext;

        // Act
        await sut.OnConnectedAsync();

        // Assert
        fakeContext.Aborts.ShouldBe(1);
    }

    [Fact]
    public async Task OnConnected_UserAlreadyConnected_AddsToGroup()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        const string connectionId = "connection1";
        using var sut = new NotificationsHub();
        var mockGroupManager = new Mock<IGroupManager>();
        sut.Groups = mockGroupManager.Object;
        sut.Context = new FakeContext(GetUser(userId), connectionId);

        // Act
        await sut.OnConnectedAsync();

        // Assert
        mockGroupManager.Verify(
            m => m.AddToGroupAsync(connectionId, userId, default));
    }

    [Fact]
    public async Task OnDisconnected_AuthenticUser_RemovesFromGroup()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        const string connectionId = "connection1";
        using var sut = new NotificationsHub();
        var mockGroupManager = new Mock<IGroupManager>();
        sut.Groups = mockGroupManager.Object;
        sut.Context = new FakeContext(GetUser(userId), connectionId);

        // Act
        await sut.OnDisconnectedAsync(null);

        // Assert
        mockGroupManager.Verify(
            m => m.RemoveFromGroupAsync(connectionId, userId, default));
    }

    private static ClaimsPrincipal GetUser(string id)
        => new([new([new(ClaimTypes.NameIdentifier, id)], "fake")]);
}
