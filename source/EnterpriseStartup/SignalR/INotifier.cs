// <copyright file="INotifier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System.Security.Claims;
using System.Threading.Tasks;

/// <summary>
/// Sends server-to-client SignalR messages.
/// </summary>
public interface INotifier
{
    /// <summary>
    /// Sends a message to a specific user, via SignalR.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="level">The notice level.</param>
    /// <param name="message">The message.</param>
    /// <returns>Async task.</returns>
    public Task Notify(ClaimsPrincipal user, NoticeLevel level, string message);
}
