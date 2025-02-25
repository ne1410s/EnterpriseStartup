// <copyright file="RedisCacheTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Caching;

using System.Text.Json;
using EnterpriseStartup.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

/// <summary>
/// Tests for the <see cref="RedisCache"/> class.
/// </summary>
public class RedisCacheTests
{
    private const StringComparison CIComparison = StringComparison.OrdinalIgnoreCase;

    [Fact]
    public async Task GetValue_NullFactory_ThrowsException()
    {
        // Arrange
        var sut = GetSut(out _, out _);

        // Act
        var act = () => sut.GetValue<string>(string.Empty, null!);

        // Assert
        (await act.ShouldThrowAsync<ArgumentNullException>())
            .ParamName.ShouldBe("factory");
    }

    [Fact]
    public async Task GetValue_FailingInnerGet_LogsError()
    {
        // Arrange
        var sut = GetSut(out var mockLogger, out var mockRedis);
        var expected = Guid.NewGuid();
        mockRedis.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), CommandFlags.None)).Throws<Exception>();
        mockRedis.Setup(m => m.KeyExistsAsync(It.IsAny<RedisKey>(), CommandFlags.None)).ReturnsAsync(true);

        // Act
        _ = await sut.GetValue("myKey", () => Task.FromResult(expected));

        // Assert
        mockLogger.VerifyLog(LogLevel.Warning, s => s!.StartsWith("Cache retrieval failed", CIComparison));
    }

    [Fact]
    public async Task GetValue_FailingInnerGetNoKey_LogsCacheMiss()
    {
        // Arrange
        var sut = GetSut(out var mockLogger, out var mockRedis);
        var expected = Guid.NewGuid();
        mockRedis.Setup(m => m.StringGetAsync(It.IsAny<RedisKey>(), CommandFlags.None)).Throws<Exception>();
        mockRedis.Setup(m => m.KeyExistsAsync(It.IsAny<RedisKey>(), CommandFlags.None)).ReturnsAsync(false);

        // Act
        _ = await sut.GetValue("myKey", () => Task.FromResult(expected));

        // Assert
        mockLogger.VerifyLog(LogLevel.Information, s => s!.StartsWith("Cache miss", CIComparison));
    }

    [Fact]
    public async Task GetValue_FailingInnerSet_LogsError()
    {
        // Arrange
        var sut = GetSut(out var mockLogger, out var mockRedis);
        var expected = Guid.NewGuid();
        mockRedis.Setup(m => m.StringSetAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), false, default, default))
                .Throws<Exception>();

        // Act
        _ = await sut.GetValue("myKey", () => Task.FromResult(expected));

        // Assert
        mockLogger.VerifyLog(LogLevel.Warning, s => s!.StartsWith("Failed to process cache", CIComparison));
    }

    [Fact]
    public async Task GetValue_DoesNotExist_FactoryCalled()
    {
        // Arrange
        var sut = GetSut(out var mockLogger, out _);
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
        var sut = GetSut(out var mockLogger, out var mockRedis);
        var expected = Guid.NewGuid();
        mockRedis.Setup(m => m.StringGetAsync("myKey", CommandFlags.None))
            .ReturnsAsync(new RedisValue(JsonSerializer.Serialize(expected)));

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
        var sut = GetSut(out _, out _);
        var expected = Guid.NewGuid();
        await sut.SetDirectly("myKey", Guid.Empty, TimeSpan.Zero);
        await Task.Delay(50);

        // Act
        var actual = await sut.GetValue("myKey", () => Task.FromResult(expected));

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task SetValue_WithExpiry_PassesValue()
    {
        // Arrange
        var sut = GetSut(out _, out var mockRedis);
        var expected = TimeSpan.FromSeconds(2);

        // Act
        await sut.SetDirectly("myKey", 42, expected);

        // Assert
        mockRedis.Verify(
            m => m.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), expected, false, default, default));
    }

    [Fact]
    public async Task TryGetDirectly_NotFound_ReturnsDefault()
    {
        // Arrange
        var sut = GetSut(out _, out _);

        // Act
        var (found, _) = await sut.TryGetDirectly<int>("myKey");

        // Assert
        found.ShouldBeFalse();
    }

    [Fact]
    public async Task TryGetDirectly_IsFound_ReturnsValue()
    {
        // Arrange
        var sut = GetSut(out _, out var mockRedis);
        const int expected = 42;
        mockRedis.Setup(m => m.StringGetAsync("myKey", CommandFlags.None))
            .ReturnsAsync(new RedisValue(JsonSerializer.Serialize(expected)));

        // Act
        var (found, actual) = await sut.TryGetDirectly<int>("myKey");

        // Assert
        found.ShouldBeTrue();
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task RemoveDirectly_WhenCalled_CallsInnerRemove()
    {
        // Arrange
        var sut = GetSut(out _, out var mockRedis);

        // Act
        _ = await sut.RemoveDirectly("myKey");

        // Assert
        mockRedis.Verify(m => m.KeyDeleteAsync("myKey", CommandFlags.None));
    }

    private static RedisCache GetSut(
        out Mock<ILogger<RedisCache>> mockLogger,
        out Mock<IDatabase> mockRedis)
    {
        mockLogger = new Mock<ILogger<RedisCache>>();
        mockRedis = new Mock<IDatabase>();
        return new RedisCache(mockLogger.Object, mockRedis.Object);
    }
}
