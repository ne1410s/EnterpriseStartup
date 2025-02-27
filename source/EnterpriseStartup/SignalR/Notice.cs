// <copyright file="Notice.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System;
using System.Collections.Generic;

/// <summary>
/// A notice.
/// </summary>
/// <param name="Level">The notice level.</param>
/// <param name="Title">The title.</param>
/// <param name="Text">The text.</param>
/// <param name="CorrelationId">Correlation id, if known.</param>
public record Notice(NoticeLevel Level, string Title, string Text, Guid? CorrelationId = null)
{
    /// <summary>
    /// Gets any supporting metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}
