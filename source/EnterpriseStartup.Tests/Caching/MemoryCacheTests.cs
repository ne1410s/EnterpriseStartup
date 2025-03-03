﻿// <copyright file="MemoryCacheTests.cs" company="ne1410s">
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
    private const StringComparison CIComparison = StringComparison.OrdinalIgnoreCase;

    [Fact]
    public void ICache_DefaultValue_IsExpected()
    {
        // Arrange & Act
        var expected = TimeSpan.FromMinutes(10);

        // Assert
        ICache.DefaultExpiry.ShouldBe(expected);
    }

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

    [Fact]
    public async Task GetValue_DoesNotExist_FactoryCalled()
    {
        // Arrange
        var sut = GetSut(out var mockLogger);
        var expected = Guid.NewGuid();

        // Act
        var actual = await sut.GetValue("myKey", () => Task.FromResult(expected));

        // Assert
        actual.ShouldBe(expected);
        mockLogger.VerifyLog(LogLevel.Information, s => s!.StartsWith("Cache miss", CIComparison));
    }

    [Fact]
    public async Task GetValue_AlreadyExists_UsesCache()
    {
        // Arrange
        var sut = GetSut(out var mockLogger);
        var expected = Guid.NewGuid();
        await sut.SetDirectly("myKey", expected);

        // Act
        var actual = await sut.GetValue("myKey", () => Task.FromResult(Guid.Empty));

        // Assert
        actual.ShouldBe(expected);
        mockLogger.VerifyLog(LogLevel.Information, s => s!.StartsWith("Cache hit", CIComparison));
    }

    [Fact]
    public async Task GetValue_ExistsButExpired_UsesFactory()
    {
        // Arrange
        var sut = GetSut(out _);
        var expected = Guid.NewGuid();
        await sut.SetDirectly("myKey", Guid.Empty, TimeSpan.Zero);
        await Task.Delay(50);

        // Act
        var actual = await sut.GetValue("myKey", () => Task.FromResult(expected));
        var (_, value) = await sut.TryGetDirectly<Guid>("myKey");

        // Assert
        actual.ShouldBe(expected);
        value.ShouldBe(expected);
    }

    [Fact]
    public async Task GetValue_ExistsButExpiredFactoryErrors_ValueRemoved()
    {
        // Arrange
        var sut = GetSut(out _);
        await sut.SetDirectly("myKey", Guid.NewGuid(), TimeSpan.Zero);
        await Task.Delay(50);

        // Act
        var act = () => sut.GetValue<Guid>("myKey", () => throw new ArgumentException("ra"));
        await act.ShouldThrowAsync<ArgumentException>();
        var (found, _) = await sut.TryGetDirectly<Guid>("myKey");

        // Assert
        found.ShouldBeFalse();
        (await sut.RemoveDirectly("myKey")).ShouldBeFalse();
    }

    [Fact]
    public async Task TryGetDirectly_NotFound_ReturnsDefault()
    {
        // Arrange
        var sut = GetSut(out _);
        const int expected = 0;

        // Act
        var (found, actual) = await sut.TryGetDirectly<int>("myKey");

        // Assert
        found.ShouldBeFalse();
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task TryGetDirectly_IsFound_ReturnsValue()
    {
        // Arrange
        var sut = GetSut(out _);
        const int expected = 42;
        await sut.SetDirectly("myKey", expected);

        // Act
        var (found, actual) = await sut.TryGetDirectly<int>("myKey");

        // Assert
        found.ShouldBeTrue();
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task TryGetDirectly_IsExpired_RemovesValue()
    {
        // Arrange
        var sut = GetSut(out _);
        await sut.SetDirectly("myKey", 42, TimeSpan.Zero);

        // Act
        await Task.Delay(50);
        var (found, actual) = await sut.TryGetDirectly<int>("myKey");

        // Assert
        found.ShouldBeFalse();
        actual.ShouldBe(0);
        (await sut.RemoveDirectly("myKey")).ShouldBeFalse();
    }

    [Fact]
    public async Task TryGetManyDirectly_IsFound_ReturnsValue()
    {
        // Arrange
        var sut = GetSut(out _);
        const int expected = 42;
        await sut.SetDirectly("myKey", expected);

        // Act
        var values = await sut.TryGetManyDirectly<int>("myKey", "nada");

        // Assert
        values.Count.ShouldBe(1);
        values["myKey"].ShouldBe(expected);
    }

    private static MemoryCache GetSut(out Mock<ILogger<MemoryCache>> mockLogger)
    {
        mockLogger = new Mock<ILogger<MemoryCache>>();
        return new MemoryCache(mockLogger.Object);
    }
}
