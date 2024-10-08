﻿// <copyright file="SignalRNotifierTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.SignalR;

using EnterpriseStartup.SignalR;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Tests for the <see cref="SignalRNotifier"/> class.
/// </summary>
public class SignalRNotifierTests
{
    [Fact]
    public async Task Notify_WhenCalled_SendsMessage()
    {
        // Arrange
        var notice = new Notice(NoticeLevel.Success, "title", "text");
        var sut = GetSut(out var mockProxy);
        object?[] expected = [notice];

        // Act
        await sut.Notify("user1", notice);

        // Assert
        mockProxy.Verify(m => m.SendCoreAsync("ReceiveMessage", expected, It.IsAny<CancellationToken>()));
    }

    private static SignalRNotifier GetSut(out Mock<IClientProxy> mockProxy)
    {
        NotificationsHub.ConnectedUsers.TryAdd("user1", ["connection1"]);
        mockProxy = new();
        var mockHubClients = new Mock<IHubClients>();
        var mockHubContext = new Mock<IHubContext<NotificationsHub>>();
        mockHubClients.Setup(m => m.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(mockProxy.Object);
        mockHubContext.Setup(m => m.Clients).Returns(mockHubClients.Object);
        return new SignalRNotifier(mockHubContext.Object);
    }
}
