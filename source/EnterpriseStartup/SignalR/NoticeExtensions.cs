// <copyright file="NoticeExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

using System.Collections.Generic;
using System.Linq;
using FluentErrors.Extensions;

/// <summary>
/// Extensions for the <see cref="Notice"/> entity.
/// </summary>
public static class NoticeExtensions
{
    /// <summary>
    /// Adds one or more metadata entries to the notice in a fluent manner.
    /// </summary>
    /// <param name="notice">The original notice.</param>
    /// <param name="entries">Metadata key-value pairs.</param>
    /// <returns>A new notice with the added metadata.</returns>
    public static Notice WithMetadata(this Notice notice, params KeyValuePair<string, object>[] entries)
    {
        return notice.MustExist() with
        {
            Metadata = notice.Metadata
                .Concat(entries)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
        };
    }
}
