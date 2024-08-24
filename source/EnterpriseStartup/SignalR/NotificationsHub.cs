// <copyright file="NotificationsHub.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR notifications hub.
/// </summary>
public class NotificationsHub : Hub
{
    /// <summary>
    /// Gets the connected users.
    /// </summary>
    public static readonly ConcurrentDictionary<string, List<string>> ConnectedUsers = [];

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        await Task.CompletedTask;
        if (this.AuthenticUser(out var username, out var connectionId))
        {
            if (!ConnectedUsers.TryGetValue(username, out _))
            {
                ConnectedUsers[username] = [connectionId];
            }
            else
            {
                ConnectedUsers[username].Add(connectionId);
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
        if (this.AuthenticUser(out var username, out var connectionId)
            && ConnectedUsers.TryGetValue(username, out _))
        {
            ConnectedUsers[username].Remove(connectionId);
            if (ConnectedUsers[username].Count == 0)
            {
                ConnectedUsers.Remove(username, out _);
            }
        }
    }

    private bool AuthenticUser(out string username, out string connectionId)
    {
        username = null!;
        connectionId = null!;

        if (this.Context.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        connectionId = this.Context.ConnectionId;
        username = this.Context.User.Identity.Name!;
        return true;
    }
}
