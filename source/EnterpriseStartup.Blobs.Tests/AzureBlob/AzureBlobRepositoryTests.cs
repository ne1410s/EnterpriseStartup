// <copyright file="AzureBlobRepositoryTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Blobs.Tests.AzureBlob;

using EnterpriseStartup.Blobs.Abstractions;
using EnterpriseStartup.Blobs.AzureBlob;

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
}