﻿// <copyright file="NotificationsHub.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System;
using System.Threading.Tasks;
using EnterpriseStartup.Auth;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR notifications hub.
/// </summary>
public class NotificationsHub : Hub
{
    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        if (!this.AuthenticUser(out var userId))
        {
            this.Context.Abort();
            return;
        }

        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, userId);

        // Stryker disable once Statement
        await base.OnConnectedAsync();
    }

    /// <inheritdoc/>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (this.AuthenticUser(out var userId))
        {
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, userId);
        }

        // Stryker disable once Statement
        await base.OnDisconnectedAsync(exception);
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