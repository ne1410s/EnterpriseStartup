// <copyright file="MqEventArgs.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Messaging.Abstractions.Consumer;

using System;
using System.Collections.Generic;

/// <summary>
/// Mq message event args.
/// </summary>
public class MqEventArgs : EventArgs
{
    /// <summary>
    /// Gets the original message.
    /// </summary>
    public string Message { get; init; } = default!;

    /// <summary>
    /// Gets the headers.
    /// </summary>
    public Dictionary<string, object> Headers { get; init; } = [];
}
