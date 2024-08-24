// <copyright file="SignalRNotifier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

/// <inheritdoc cref="INotifier"/>
public class SignalRNotifier(IHubContext<NotificationsHub> hubContext) : INotifier
{
    /// <inheritdoc/>
    public async Task Notify(ClaimsPrincipal user, NoticeLevel level, string message)
    {
        if (user?.Identity?.IsAuthenticated == true)
        {
            var connectionIds = NotificationsHub.ConnectedUsers[user.Identity.Name!];
            var clients = hubContext.Clients.Clients(connectionIds);
            await clients.SendAsync("ReceiveMessage", level, message);
        }
    }
}
