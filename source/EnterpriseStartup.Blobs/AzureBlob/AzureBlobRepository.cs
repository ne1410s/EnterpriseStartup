// <copyright file="AzureBlobRepository.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Blobs.AzureBlob;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnterpriseStartup.Blobs.Abstractions;
using EnterpriseStartup.Utils.Pagination;
using FluentErrors.Extensions;
using Microsoft.Extensions.Azure;

/// <inheritdoc cref="IUserBlobRepository"/>
public class AzureBlobRepository(IAzureClientFactory<BlobServiceClient> clientFactory) : IUserBlobRepository
{
    /// <inheritdoc/>
    public async Task<Guid> UploadAsync(
        string containerName, string userId, BlobData data, bool ephemeral = false)
    {
        data.MustExist().MetaData.MustExist();
        var container = this.GetContainer(containerName, ephemeral);
        await container.CreateIfNotExistsAsync();

        var blobReference = Guid.NewGuid();
        var metadata = new Dictionary<string, string> { ["filename"] = data.MetaData.FileName };
        var blob = container.GetBlobClient($"{userId}/{blobReference}");
        var uploadResult = await blob.UploadAsync(data.Content, new BlobUploadOptions { Metadata = metadata });
        uploadResult.GetRawResponse().IsError.MustBe(false);

        return blobReference;
    }

    /// <inheritdoc/>
    public async Task<LazyPageResult<BlobMetaData>> ListAsync(
        string containerName, string userId, PageRequest request, bool ephemeral = false)
    {
        request.MustExist();
        var container = this.GetContainer(containerName, ephemeral);
        var data = new List<BlobMetaData>();
        if (await container.ExistsAsync())
        {
            var skip = (request.PageNumber - 1) * request.PageSize;
            var pageable = container.GetBlobsAsync(prefix: $"{userId}/");
            await foreach (var blob in pageable.Skip(skip).Take(request.PageSize))
            {
                var size = blob.Properties.ContentLength ?? 0;
                var blobName = blob.Metadata["filename"];
                var blobRef = Guid.Parse(blob.Name.Split('/')[^1]);
                data.Add(new(blobRef, blob.Properties.ContentType, blobName, size));
            }
        }

        return new()
        {
            Data = data,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
        };
    }

    /// <inheritdoc/>
    public async Task<BlobData> DownloadAsync(
        string containerName, string userId, Guid blobReference, bool ephemeral = false)
    {
        var container = this.GetContainer(containerName, ephemeral);
        var blob = container.GetBlobClient($"{userId}/{blobReference}");
        var stream = new MemoryStream();
        var result = await blob.DownloadToAsync(stream);
        result.IsError.MustBe(false);

        var props = (await blob.GetPropertiesAsync()).Value;
        var name = props.Metadata["filename"];
        var size = props.ContentLength;
        stream.Position = 0;
        return new(stream, new(blobReference, props.ContentType, name, size));
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(
        string containerName, string userId, Guid blobReference, bool ephemeral = false)
    {
        var container = this.GetContainer(containerName, ephemeral);
        var blob = container.GetBlobClient($"{userId}/{blobReference}");
        var result = await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        result.GetRawResponse().IsError.MustBe(false);
    }

    private BlobContainerClient GetContainer(string name, bool ephemeral)
    {
        var serviceClient = ephemeral
            ? clientFactory.CreateClient(IUserBlobRepository.Ephemeral)
            : clientFactory.CreateClient(IUserBlobRepository.Permanent);

        return serviceClient.GetBlobContainerClient(name);
    }
}
