// <copyright file="TestObjects.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Blobs.Tests.AzureBlob;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnterpriseStartup.Blobs.Abstractions;
using EnterpriseStartup.Blobs.AzureBlob;
using Microsoft.Extensions.Azure;

internal static class TestHelper
{
    public static AzureBlobRepository GetMockRepo(
        bool containerExists, out FakeServiceClient service, bool blobError = false)
    {
        var mockFactory = new Mock<IAzureClientFactory<BlobServiceClient>>();
        var svc = new FakeServiceClient(containerExists, blobError);
        mockFactory
            .Setup(m => m.CreateClient(It.IsAny<string>()))
            .Returns((string s) =>
            {
                svc.Name = s;
                return svc;
            });

        service = svc;
        return new(mockFactory.Object);
    }

    public static BlobMetaData GetTestMeta() => new(Guid.Empty, "c1", "f1", 1);

    public static BlobData GetTestBlob() => new(new MemoryStream(), GetTestMeta());
}

public class FakeServiceClient(bool containerExists, bool blobError) : BlobServiceClient
{
    public string Name { get; set; } = default!;

    public FakeContainerClient? FakeContainer { get; set; }

    public override BlobContainerClient GetBlobContainerClient(string blobContainerName)
    {
        this.FakeContainer = new FakeContainerClient(containerExists, blobError);
        return this.FakeContainer;
    }
}

public class FakeContainerClient(bool containerExists, bool blobError) : BlobContainerClient
{
    private static readonly Guid Guid01 = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Guid02 = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid Guid03 = Guid.Parse("00000000-0000-0000-0000-000000000003");

    public Collection<string> Calls { get; } = [];

    public Collection<BlobUploadOptions> Uploads { get; } = [];

    public override Task<Response<BlobContainerInfo>> CreateIfNotExistsAsync(
        PublicAccessType publicAccessType = PublicAccessType.None,
        IDictionary<string, string> metadata = null!,
        BlobContainerEncryptionScopeOptions encryptionScopeOptions = null!,
        CancellationToken cancellationToken = default)
    {
        this.Calls.Add(nameof(this.CreateIfNotExistsAsync));
        return Task.FromResult<Response<BlobContainerInfo>>(null!);
    }

    public override BlobClient GetBlobClient(string blobName)
    {
        this.Calls.Add($"{nameof(this.GetBlobClient)}_{blobName}");
        return new FakeBlobClient(this, blobError);
    }

    public override Task<Response<bool>> ExistsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<Response<bool>>(new FakeResponse<bool>(containerExists, false));

    public override AsyncPageable<BlobItem> GetBlobsAsync(
        BlobTraits traits = BlobTraits.None,
        BlobStates states = BlobStates.None,
        string prefix = null!,
        CancellationToken cancellationToken = default)
    {
        this.Calls.Add($"{nameof(this.GetBlobsAsync)}_{prefix}");
        var fakeProps = BlobsModelFactory.BlobItemProperties(true, contentLength: 212);
        var fakeMeta1 = new Dictionary<string, string> { ["filename"] = "mf1" };
        var fakeItem1 = BlobsModelFactory.BlobItem($"b1/{Guid.Empty}", properties: fakeProps, metadata: fakeMeta1);
        var fakePage1 = new FakePage<BlobItem>(fakeItem1);
        var fakeMeta2 = new Dictionary<string, string> { ["filename"] = "mf2" };
        var fakeItem2 = BlobsModelFactory.BlobItem($"b2/{Guid.Empty}", properties: fakeProps, metadata: fakeMeta2);
        var fakePage2 = new FakePage<BlobItem>(fakeItem2);
        var fakeMeta3 = new Dictionary<string, string> { ["filename"] = "mf3" };
        var fakeItem3 = BlobsModelFactory.BlobItem($"b3/{Guid.Empty}", properties: fakeProps, metadata: fakeMeta3);
        var fakePage3 = new FakePage<BlobItem>(fakeItem3);
        var fakeItem4 = BlobsModelFactory.BlobItem($"b4/{Guid01}", properties: fakeProps, metadata: null);
        var fakeMeta5 = new Dictionary<string, string> { ["filename"] = string.Empty };
        var fakeItem5 = BlobsModelFactory.BlobItem($"b5/{Guid02}", properties: fakeProps, metadata: fakeMeta5);
        var fakeMeta6 = new Dictionary<string, string>();
        var fakeItem6 = BlobsModelFactory.BlobItem($"b6/{Guid03}", properties: fakeProps, metadata: fakeMeta6);
        var fakePage4 = new FakePage<BlobItem>(fakeItem4, fakeItem5, fakeItem6);
        return AsyncPageable<BlobItem>.FromPages([fakePage1, fakePage2, fakePage3, fakePage4]);
    }
}

[ExcludeFromCodeCoverage]
public class FakePage<T>(params T[] items) : Page<T>
{
    public override IReadOnlyList<T> Values => items;

    public override string? ContinuationToken => default;

    public override Response GetRawResponse() => new FakeResponse(false);
}

public class FakeBlobClient(FakeContainerClient parent, bool blobError) : BlobClient
{
    public override Task<Response<BlobContentInfo>> UploadAsync(
        Stream content,
        BlobUploadOptions options,
        CancellationToken cancellationToken = default)
    {
        parent.Uploads.Add(options);
        return Task.FromResult<Response<BlobContentInfo>>(new FakeResponse<BlobContentInfo>(default!, blobError));
    }

    public override Task<Response> DownloadToAsync(Stream destination)
    {
        return Task.FromResult<Response>(new FakeResponse(blobError));
    }

    public override Task<Response<BlobProperties>> GetPropertiesAsync(
        BlobRequestConditions conditions = null!,
        CancellationToken cancellationToken = default)
    {
        var fakeMeta = new Dictionary<string, string> { ["filename"] = "mf1" };
        var fakeProps = BlobsModelFactory.BlobProperties(contentLength: 212, metadata: fakeMeta);
        return Task.FromResult<Response<BlobProperties>>(new FakeResponse<BlobProperties>(fakeProps, false));
    }

    public override Task<Response<bool>> DeleteIfExistsAsync(
        DeleteSnapshotsOption snapshotsOption = DeleteSnapshotsOption.None,
        BlobRequestConditions conditions = null!,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Response<bool>>(new FakeResponse<bool>(true, blobError));
    }
}

public class FakeResponse<T>(T value, bool error) : Response<T>
{
    public override T Value => value;

    public override Response GetRawResponse() => new FakeResponse(error);
}

[ExcludeFromCodeCoverage]
public class FakeResponse(bool error) : Response
{
    public override int Status => default;

    public override bool IsError => error;

    public override string ReasonPhrase => default!;

    public override Stream? ContentStream { get; set; }

    public override string ClientRequestId { get; set; } = default!;

    [SuppressMessage("Usage", "CA1816", Justification = "Because")]
    public override void Dispose()
    { }

    protected override bool ContainsHeader(string name) => default;

    protected override IEnumerable<HttpHeader> EnumerateHeaders() => default!;

    protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value)
    {
        value = default;
        return default;
    }

    protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values)
    {
        values = default;
        return default;
    }
}
