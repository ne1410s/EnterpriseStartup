// <copyright file="MemoryCacheTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Caching;

using EnterpriseStartup.Caching;
using Microsoft.Extensions.Logging;

/// <summary>
/// Tests for the <see cref="MemoryCache"/> class.
/// </summary>
public class MemoryCacheTests
{
    [Fact]
    public async Task GetValue_NullFactory_ThrowsException()
    {
        // Arrange
        var sut = GetSut(out _);

        // Act
        var act = () => sut.GetValue<string>(string.Empty, null!);

        // Assert
        (await act.ShouldThrowAsync<ArgumentNullException>())
            .ParamName.ShouldBe("factory");
    }

    private static MemoryCache GetSut(out Mock<ILogger<MemoryCache>> mockLogger)
    {
        mockLogger = new Mock<ILogger<MemoryCache>>();
        return new MemoryCache(mockLogger.Object);
    }
}
