// <copyright file="SignalRInMemNotifierTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.SignalR;

using EnterpriseStartup.SignalR;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// Tests for the <see cref="SignalRInMemNotifier"/> class.
/// </summary>
public class SignalRInMemNotifierTests
{
    [Fact]
    public async Task Notify_WhenCalled_SendsMessage()
    {
        // Arrange
        var notice = new Notice(NoticeLevel.Success, "title", "text");
        var sut = GetSut(out var mockProxy);
        object?[] expected = [notice];

        // Act
        await sut.Notify(notice, "user1");

        // Assert
        mockProxy.Verify(m => m.SendCoreAsync("ReceiveMessage", expected, It.IsAny<CancellationToken>()));
    }

    private static SignalRInMemNotifier GetSut(out Mock<IClientProxy> mockProxy)
    {
        NotificationsHubInMem.ConnectedUsers["user1"] = ["blah"];

        mockProxy = new();
        var mockHubClients = new Mock<IHubClients>();
        var mockHubContext = new Mock<IHubContext<NotificationsHubInMem>>();
        mockHubClients.Setup(m => m.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(mockProxy.Object);
        mockHubContext.Setup(m => m.Clients).Returns(mockHubClients.Object);
        return new SignalRInMemNotifier(mockHubContext.Object);
    }
}
