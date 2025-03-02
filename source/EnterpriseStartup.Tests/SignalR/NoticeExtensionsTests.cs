// <copyright file="NoticeExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.SignalR;

using EnterpriseStartup.SignalR;

/// <summary>
/// Tests for the <see cref="NoticeExtensions"/> class.
/// </summary>
public class NoticeExtensionsTests
{
    [Fact]
    public void WithMetadata_NewValues_MergesThem()
    {
        // Arrange
        var notice = new Notice(NoticeLevel.Neutral, "Test", "Title") { Metadata = { ["one"] = 1 } };

        // Act
        var result = notice.WithMetadata(new("two", 2), new("three", 3));

        // Assert
        result.Metadata.Count.ShouldBe(3);
    }
}
