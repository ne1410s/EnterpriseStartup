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
    public static readonly ConcurrentDictionary<Guid, List<string>> ConnectedUsers = [];

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        await Task.CompletedTask;
        if (this.AuthenticUser(out var userId, out var connectionId))
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
        if (this.AuthenticUser(out var userId, out var connectionId)
            && ConnectedUsers.TryGetValue(userId, out _))
        {
            ConnectedUsers[userId].Remove(connectionId);
            if (ConnectedUsers[userId].Count == 0)
            {
                ConnectedUsers.Remove(userId, out _);
            }
        }
    }

    private bool AuthenticUser(out Guid userId, out string connectionId)
    {
        userId = Guid.Empty;
        connectionId = null!;

        if (this.Context.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        connectionId = this.Context.ConnectionId;

        // TODO: Get user id from OID/claim
        userId = Guid.NewGuid();
        return true;
    }
}
