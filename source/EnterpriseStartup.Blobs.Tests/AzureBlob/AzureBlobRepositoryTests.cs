// <copyright file="AzureBlobRepositoryTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Blobs.Tests.AzureBlob;

using Azure.Storage.Blobs;
using EnterpriseStartup.Blobs.Abstractions;
using EnterpriseStartup.Blobs.AzureBlob;
using FluentErrors.Errors;

/// <summary>
/// Tests for the <see cref="AzureBlobRepository"/> class.
/// </summary>
public class AzureBlobRepositoryTests
{
    [Fact]
    public async Task UploadAsync_WhenCalled_ReturnsExpected()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _);
        var testBlob = TestHelper.GetTestBlob();

        // Act
        var result = await mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task UploadAsync_BlobDataNull_ThrowsException()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _);
        var testBlob = new BlobData(new MemoryStream(), null!);

        // Act
        var act = () => mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        _ = await act.ShouldThrowAsync<DataStateException>();
    }

    [Fact]
    public async Task UploadAsync_BlobError_ThrowsException()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _, true);
        var testBlob = TestHelper.GetTestBlob();

        // Act
        var act = () => mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        _ = await act.ShouldThrowAsync<DataStateException>();
    }

    [Fact]
    public async Task UploadAsync_WhenCalled_CreatesContainerIfNotExists()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);
        var testBlob = TestHelper.GetTestBlob();

        // Act
        _ = await mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        service.FakeContainer!.Calls.ShouldContain(nameof(BlobContainerClient.CreateIfNotExistsAsync));
    }

    [Fact]
    public async Task UploadAsync_WhenCalled_GetsBlobClient()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);
        var testBlob = TestHelper.GetTestBlob();

        // Act
        _ = await mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        service.FakeContainer!.Calls.ShouldContain(s => s.StartsWith("GetBlobClient_u/"));
    }

    [Fact]
    public async Task UploadAsync_WhenCalled_PassesExpectedOptions()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);
        var testBlob = TestHelper.GetTestBlob();
        var expectedMeta = new Dictionary<string, string> { ["filename"] = "f1" };

        // Act
        _ = await mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        service.FakeContainer!.Uploads[0].Metadata.ShouldBeEquivalentTo(expectedMeta);
    }

    [Fact]
    public async Task ListAsync_NullRequest_ThrowsException()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: false, out _);

        // Act
        var act = () => mockRepo.ListAsync("c", "u", null!);

        // Assert
        _ = await act.ShouldThrowAsync<DataStateException>();
    }

    [Fact]
    public async Task ListAsync_NoContainer_ReturnsEmpty()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: false, out _);

        // Act
        var result = await mockRepo.ListAsync("c", "u", new());

        // Assert
        result.Data.ShouldBeEmpty();
        result.PageSize.ShouldBe(100);
        result.PageNumber.ShouldBe(1);
    }

    [Fact]
    public async Task ListAsync_WithContainer_GetsBlobs()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: true, out var service);

        // Act
        _ = await mockRepo.ListAsync("c", "u", new());

        // Assert
        service.FakeContainer!.Calls.ShouldContain(s => s.StartsWith("GetBlobsAsync_u/"));
    }

    [Fact]
    public async Task ListAsync_WithPaging_ReturnsExpected()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: true, out _);

        // Act
        var result = await mockRepo.ListAsync("c", "u", new(2, 2));

        // Assert
        result.Data.First().FileName.ShouldBe("mf3");
    }

    [Fact]
    public async Task ListAsync_WithContainer_ReturnsData()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: true, out _);

        // Act
        var result = await mockRepo.ListAsync("c", "u", new());

        // Assert
        result.Data.First().FileSize.ShouldBe(212);
    }

    [Fact]
    public async Task ListAsync_MissingMetadata_StillReturnsData()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: true, out _);

        // Act
        var result = await mockRepo.ListAsync("c", "u", new(2, 3));

        // Assert
        result.Data.ElementAt(0).FileName.ShouldEndWith("1");
        result.Data.ElementAt(1).FileName.ShouldEndWith("2");
        result.Data.ElementAt(2).FileName.ShouldEndWith("3");
    }

    [Fact]
    public async Task DownloadAsync_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _);

        // Act
        var result = await mockRepo.DownloadAsync("c", "u", Guid.Empty);

        // Assert
        _ = result.MetaData.ShouldNotBeNull();
    }

    [Fact]
    public async Task DownloadAsync_WhenCalled_GetsBlobClient()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        _ = await mockRepo.DownloadAsync("c", "u", Guid.Empty);

        // Assert
        service.FakeContainer!.Calls.ShouldContain(s => s.StartsWith("GetBlobClient_u/"));
    }

    [Fact]
    public async Task DownloadAsync_BlobError_ThrowsException()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _, true);

        // Act
        var act = () => mockRepo.DownloadAsync("c", "u", Guid.Empty);

        // Assert
        _ = await act.ShouldThrowAsync<DataStateException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _);

        // Act
        var act = () => mockRepo.DeleteAsync("c", "u", Guid.Empty);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_EphemeralStorage_CallsExpectedName()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        await mockRepo.DeleteAsync("c", "u", Guid.Empty, ephemeral: true);

        // Assert
        service.Name.ShouldBe(IUserBlobRepository.Ephemeral);
    }

    [Fact]
    public async Task DeleteAsync_PermanentStorage_CallsExpectedName()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        await mockRepo.DeleteAsync("c", "u", Guid.Empty, ephemeral: false);

        // Assert
        service.Name.ShouldBe(IUserBlobRepository.Permanent);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_GetsBlobClient()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        await mockRepo.DeleteAsync("c", "u", Guid.Empty);

        // Assert
        service.FakeContainer!.Calls.ShouldContain(s => s.StartsWith("GetBlobClient_u/"));
    }

    [Fact]
    public async Task DeleteAsync_BlobError_ThrowsException()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _, true);

        // Act
        var act = () => mockRepo.DeleteAsync("c", "u", Guid.Empty);

        // Assert
        _ = await act.ShouldThrowAsync<DataStateException>();
    }
}