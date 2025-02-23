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

    private static RedisCache GetSut(
        out Mock<ILogger<RedisCache>> mockLogger,
        out Mock<IDatabase> mockRedis)
    {
        mockLogger = new Mock<ILogger<RedisCache>>();
        mockRedis = new Mock<IDatabase>();
        return new RedisCache(mockLogger.Object, mockRedis.Object);
    }
}
