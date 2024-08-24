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
    Neutral = 0,

    /// <summary>
    /// Success notices.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Failure notices.
    /// </summary>
    Failure = 2,

    /// <summary>
    /// System errors.
    /// </summary>
    SystemError = 3,
}
