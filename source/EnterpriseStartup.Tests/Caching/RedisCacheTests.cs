// <copyright file="RedisCacheTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Tests.Caching;

using EnterpriseStartup.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

/// <summary>
/// Tests for the <see cref="RedisCache"/> class.
/// </summary>
public class RedisCacheTests
{
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
    public async Task GetValue_DoesNotExist_FactoryCalled()
    {
        // Arrange
        var sut = GetSut(out _, out _);
        var expected = Guid.NewGuid();

        // Act
        var actual = await sut.GetValue("myKey", () => Task.FromResult(expected));

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task GetValue_AlreadyExists_UsesCache()
    {
        // Arrange
        var sut = GetSut(out _, out _);
        var expected = Guid.NewGuid();
        await sut.SetDirectly("myKey", expected);

        // Act
        var actual = await sut.GetValue("myKey", () => Task.FromResult(Guid.Empty));

        // Assert
        actual.ShouldBe(expected);
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
    public async Task GetDirectly_NotFound_ReturnsDefault()
    {
        // Arrange
        var sut = GetSut(out _, out _);
        const int expected = 0;

        // Act
        var actual = await sut.GetDirectly<int>("myKey");

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task GetDirectly_IsFound_ReturnsValue()
    {
        // Arrange
        var sut = GetSut(out _, out _);
        const int expected = 42;
        await sut.SetDirectly("myKey", expected);

        // Act
        var actual = await sut.GetDirectly<int>("myKey");

        // Assert
        actual.ShouldBe(expected);
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
