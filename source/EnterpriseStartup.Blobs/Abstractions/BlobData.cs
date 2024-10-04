// <copyright file="BlobData.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Blobs.Abstractions;

using System.IO;

/// <summary>
/// Associates a raw blob stream with accompanying metadata.
/// </summary>
/// <param name="Content">The content stream.</param>
/// <param name="MetaData">The blob metadata.</param>
public record BlobData(Stream Content, BlobMetaData MetaData);
