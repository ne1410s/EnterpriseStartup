// <copyright file="NotificationsHub.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseStartup.Auth;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR notifications hub.
/// </summary>
public class NotificationsHub : Hub
{
    /// <summary>
    /// Connected users, by user id.
    /// </summary>
    public static readonly ConcurrentDictionary<string, List<string>> ConnectedUsers = [];

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        await Task.CompletedTask;
        var connectionId = this.Context.ConnectionId;
        if (this.AuthenticUser(out var userId))
        {
            if (!ConnectedUsers.TryGetValue(userId, out _))
            {
                ConnectedUsers[userId] = [connectionId];
            }
            else
            {
                ConnectedUsers[userId].Add(connectionId);
            }
        }
        else
        {
            this.Context.Abort();
        }
    }

    /// <inheritdoc/>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Task.CompletedTask;
        if (this.AuthenticUser(out var userId))
        {
            ConnectedUsers[userId].Remove(this.Context.ConnectionId);
            if (ConnectedUsers[userId].Count == 0)
            {
                ConnectedUsers.Remove(userId, out _);
            }
        }
    }

    private bool AuthenticUser(out string userId)
    {
        var principal = this.Context.User;
        var hasAuth = principal?.Identity?.IsAuthenticated == true;
        userId = hasAuth ? principal!.ToUser().Id : string.Empty;
        return hasAuth;
    }
}
