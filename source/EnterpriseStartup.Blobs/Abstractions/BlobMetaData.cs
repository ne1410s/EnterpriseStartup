// <copyright file="BlobMetaData.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Blobs.Abstractions;

using System;

/// <summary>
/// Blob metadata.
/// </summary>
/// <param name="Reference">The unique blob reference.</param>
/// <param name="ContentType">The content type.</param>
/// <param name="FileName">The file name.</param>
/// <param name="FileSize">The size of the file, in bytes.</param>
public record BlobMetaData(Guid Reference, string ContentType, string FileName, long FileSize);
