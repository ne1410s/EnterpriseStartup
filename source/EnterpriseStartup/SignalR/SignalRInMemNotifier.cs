// <copyright file="SignalRInMemNotifier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

/// <inheritdoc cref="INotifier"/>
public class SignalRInMemNotifier(IHubContext<NotificationsHubInMem> hubContext) : INotifier
{
    /// <inheritdoc/>
    public async Task Notify(string userId, Notice notice)
    {
        if (NotificationsHubInMem.ConnectedUsers.TryGetValue(userId, out var connectionIds))
        {
            var proxy = hubContext.Clients.Clients(connectionIds);
            await proxy.SendAsync("ReceiveMessage", notice);
        }
    }
}
