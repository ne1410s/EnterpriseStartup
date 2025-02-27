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
    /// <param name="notice">The notice.</param>
    /// <param name="recipientIds">The recipient user ids.</param>
    /// <returns>Async task.</returns>
    public Task Notify(Notice notice, params string[] recipientIds);
}
