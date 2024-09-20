// <copyright file="INotifier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System.Threading.Tasks;

/// <summary>
/// Sends server-to-client SignalR messages.
/// </summary>
public interface INotifier
{
    /// <summary>
    /// Notifies a specific user, via SignalR.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="notice">The notice.</param>
    /// <returns>Async task.</returns>
    public Task Notify(string userId, Notice notice);
}
