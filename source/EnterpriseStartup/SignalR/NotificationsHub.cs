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

    private bool AuthenticUser(out string userId, out string connectionId)
    {
        userId = null!;
        connectionId = null!;

        var principal = this.Context.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        userId = principal.ToUser().Id;
        connectionId = this.Context.ConnectionId;
        return true;
    }
}
