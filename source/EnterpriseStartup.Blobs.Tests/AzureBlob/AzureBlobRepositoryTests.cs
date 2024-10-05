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
        result.Should().NotBe(Guid.Empty);
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
        await act.Should().ThrowAsync<DataStateException>();
    }

    [Fact]
    public async Task UploadAsync_WhenCalled_CreatesContainerIfNotExists()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);
        var testBlob = TestHelper.GetTestBlob();

        // Act
        await mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        service.FakeContainer!.Calls.Should().Contain(nameof(BlobContainerClient.CreateIfNotExistsAsync));
    }

    [Fact]
    public async Task UploadAsync_WhenCalled_GetsBlobClient()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);
        var testBlob = TestHelper.GetTestBlob();

        // Act
        await mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        service.FakeContainer!.Calls.Should().Contain(s => s.StartsWith("GetBlobClient_u/"));
    }

    [Fact]
    public async Task UploadAsync_WhenCalled_PassesExpectedOptions()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);
        var testBlob = TestHelper.GetTestBlob();
        var expectedMeta = new Dictionary<string, string> { ["filename"] = "f1" };

        // Act
        await mockRepo.UploadAsync("c", "u", testBlob);

        // Assert
        service.FakeContainer!.Uploads[0].Metadata.Should().BeEquivalentTo(expectedMeta);
    }

    [Fact]
    public async Task ListAsync_NoContainer_ReturnsEmpty()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: false, out _);

        // Act
        var result = await mockRepo.ListAsync("c", "u", new());

        // Assert
        result.Data.Should().BeEmpty();
        result.PageSize.Should().Be(100);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task ListAsync_WithContainer_ReturnsData()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(containerExists: true, out _);

        // Act
        var result = await mockRepo.ListAsync("c", "u", new());

        // Assert
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DownloadAsync_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _);

        // Act
        var result = await mockRepo.DownloadAsync("c", "u", Guid.Empty);

        // Assert
        result.MetaData.Should().NotBeNull();
    }

    [Fact]
    public async Task DownloadAsync_WhenCalled_GetsBlobClient()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        await mockRepo.DownloadAsync("c", "u", Guid.Empty);

        // Assert
        service.FakeContainer!.Calls.Should().Contain(s => s.StartsWith("GetBlobClient_u/"));
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out _);

        // Act
        var act = () => mockRepo.DeleteAsync("c", "u", Guid.Empty);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_EphemeralStorage_CallsExpectedName()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        await mockRepo.DeleteAsync("c", "u", Guid.Empty, ephemeral: true);

        // Assert
        service.Name.Should().Be(IUserBlobRepository.Ephemeral);
    }

    [Fact]
    public async Task DeleteAsync_PermanentStorage_CallsExpectedName()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        await mockRepo.DeleteAsync("c", "u", Guid.Empty, ephemeral: false);

        // Assert
        service.Name.Should().Be(IUserBlobRepository.Permanent);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_GetsBlobClient()
    {
        // Arrange
        var mockRepo = TestHelper.GetMockRepo(true, out var service);

        // Act
        await mockRepo.DeleteAsync("c", "u", Guid.Empty);

        // Assert
        service.FakeContainer!.Calls.Should().Contain(s => s.StartsWith("GetBlobClient_u/"));
    }
}