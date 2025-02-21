// <copyright file="RedisCache.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Caching;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

/// <summary>
/// Redis caching implementation.
/// </summary>
/// <param name="logger">A logger.</param>
/// <param name="redis">Redis store.</param>
public class RedisCache(ILogger<RedisCache> logger, IDatabase redis) : ICache
{
    private static readonly SemaphoreSlim CacheLock = new(1, 1);

    /// <inheritdoc/>
    public async Task<T?> GetValue<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        factory = factory ?? throw new ArgumentNullException(nameof(factory));

        T? retVal;
        try
        {
            retVal = await this.GetDirectly<T>(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache retrieval failed for key: {Key}", key);
            retVal = default;
        }

        if (retVal is null && !await redis.KeyExistsAsync(key))
        {
            logger.LogInformation("Cache MISS on: {Key}", key);

            await CacheLock.WaitAsync();
            try
            {
                // Double-check in case another thread already populated it
                retVal = await this.GetDirectly<T>(key);
                if (retVal is null)
                {
                    retVal = await factory();

                    try
                    {
                        await this.SetDirectly(key, retVal, expiry);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to set cache for key: {Key}", key);
                    }
                }
            }
            finally
            {
                CacheLock.Release();
            }
        }
        else
        {
            logger.LogInformation("Cache HIT on: {Key}", key);
        }

        return retVal;
    }

    /// <inheritdoc/>
    public async Task<T?> GetDirectly<T>(string key)
    {
        var value = await redis.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    /// <inheritdoc/>
    public async Task SetDirectly<T>(string key, T value, TimeSpan? expiry = null)
    {
        var jsonValue = JsonSerializer.Serialize(value);
        await redis.StringSetAsync(key, jsonValue, expiry ?? ICache.DefaultExpiry);
    }

    /// <inheritdoc/>
    public async Task RemoveDirectly(string key) => await redis.KeyDeleteAsync(key);
}
