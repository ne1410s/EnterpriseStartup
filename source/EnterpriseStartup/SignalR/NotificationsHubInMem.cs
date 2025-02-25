// <copyright file="NotificationsHubInMem.cs" company="ne1410s">
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
/// SignalR notifications hub (in memory).
/// </summary>
public class NotificationsHubInMem : Hub
{
    /// <summary>
    /// Connected users, by user id.
    /// </summary>
    public static readonly ConcurrentDictionary<string, List<string>> ConnectedUsers = [];

    /// <inheritdoc/>
    public override Task OnConnectedAsync()
    {
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

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (this.AuthenticUser(out var userId))
        {
            ConnectedUsers[userId].Remove(this.Context.ConnectionId);
            if (ConnectedUsers[userId].Count == 0)
            {
                ConnectedUsers.Remove(userId, out _);
            }
        }

        return Task.CompletedTask;
    }

    private bool AuthenticUser(out string userId)
    {
        userId = null!;
        var principal = this.Context.User;
        var hasAuth = principal?.Identity?.IsAuthenticated == true;
        if (hasAuth)
        {
            userId = principal!.ToEnterpriseUser().Id;
        }

        return hasAuth;
    }
}
