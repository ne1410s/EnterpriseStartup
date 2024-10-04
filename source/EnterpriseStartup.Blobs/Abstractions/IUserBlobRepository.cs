// <copyright file="IUserBlobRepository.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Blobs.Abstractions;

using System.Threading.Tasks;
using System;
using EnterpriseStartup.Utils.Pagination;

/// <summary>
/// A blob repository that organises blobs by user id.
/// </summary>
public interface IUserBlobRepository
{
    /// <summary>
    /// Name of the ephemeral blob storage account.
    /// </summary>
    public const string Ephemeral = "Ephemeral";

    /// <summary>
    /// Name of the permanent blob storage account.
    /// </summary>
    public const string Permanent = "Permanent";

    /// <summary>
    /// Uploads a new blob.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="data">The blob data.</param>
    /// <param name="ephemeral">Whether to use ephemeral storage.</param>
    /// <returns>The newly-generated blob reference.</returns>
    public Task<Guid> UploadAsync(
        string containerName, string userId, BlobData data, bool ephemeral = false);

    /// <summary>
    /// Downloads a blob.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="blobReference">The blob reference.</param>
    /// <param name="ephemeral">Whether to use ephemeral storage.</param>
    /// <returns>The blob data.</returns>
    public Task<BlobData> DownloadAsync(
        string containerName, string userId, Guid blobReference, bool ephemeral = false);

    /// <summary>
    /// Lists blobs using pagination.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="request">The paging request.</param>
    /// <param name="ephemeral">Whether to use ephemeral storage.</param>
    /// <returns>A page of data.</returns>
    public Task<LazyPageResult<BlobMetaData>> ListAsync(
        string containerName, string userId, PageRequest request, bool ephemeral = false);

    /// <summary>
    /// Deletes a blob.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="blobReference">The blob reference.</param>
    /// <param name="ephemeral">Whether to use ephemeral storage.</param>
    /// <returns>Async task.</returns>
    public Task DeleteAsync(
        string containerName, string userId, Guid blobReference, bool ephemeral = false);
}
