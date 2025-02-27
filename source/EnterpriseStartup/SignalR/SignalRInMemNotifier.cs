// <copyright file="SignalRInMemNotifier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

/// <inheritdoc cref="INotifier"/>
public class SignalRInMemNotifier(IHubContext<NotificationsHubInMem> hubContext) : INotifier
{
    /// <inheritdoc/>
    public async Task Notify(Notice notice, params string[] recipientIds)
    {
        var userList = recipientIds.ToList();
        var allClientConnections = NotificationsHubInMem.ConnectedUsers
            .Where(u => userList.Contains(u.Key))
            .SelectMany(u => u.Value);

        var proxy = hubContext.Clients.Clients(allClientConnections);
        await proxy.SendAsync("ReceiveMessage", notice);
    }
}
