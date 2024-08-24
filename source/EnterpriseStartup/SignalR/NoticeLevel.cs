// <copyright file="NoticeLevel.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

/// <summary>
/// Notice level for a notification.
/// </summary>
public enum NoticeLevel
{
    /// <summary>
    /// Information notices.
    /// </summary>
    Information = 0,

    /// <summary>
    /// Warning notices.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error notices.
    /// </summary>
    Error = 2,
}
