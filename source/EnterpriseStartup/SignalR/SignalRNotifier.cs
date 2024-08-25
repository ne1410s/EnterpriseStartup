// <copyright file="SignalRNotifier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

/// <inheritdoc cref="INotifier"/>
public class SignalRNotifier(IHubContext<NotificationsHub> hubContext) : INotifier
{
    /// <inheritdoc/>
    public async Task Notify(string userId, NoticeLevel level, string message)
    {
        var connectionIds = NotificationsHub.ConnectedUsers[userId];
        var clients = hubContext.Clients.Clients(connectionIds);
        await clients.SendAsync("ReceiveMessage", level, message);
    }
}
