// <copyright file="Notice.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.SignalR;

/// <summary>
/// A notice.
/// </summary>
/// <param name="Level">The notice level.</param>
/// <param name="Title">The title.</param>
/// <param name="Text">The text.</param>
/// <param name="Data">Any supporting data.</param>
public record Notice(NoticeLevel Level, string Title, string Text, object? Data = null);
