// <copyright file="MemoryCache.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Caching;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

/// <summary>
/// Memory caching implementation. Useful for local development / testing.
/// </summary>
public class MemoryCache(ILogger<MemoryCache> logger) : ICache
{
    private readonly ConcurrentDictionary<string, (object? Value, DateTimeOffset Expiry)> cache = new();

    /// <inheritdoc/>
    public async Task<T?> GetValue<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        factory = factory ?? throw new ArgumentNullException(nameof(factory));
        if (this.cache.TryGetValue(key, out var cacheEntry))
        {
            // Stryker disable all
            if (DateTimeOffset.UtcNow < cacheEntry.Expiry)
            {
                logger.LogInformation("Cache HIT on: {Key}", key);
                return (T?)cacheEntry.Value;
            }
            else
            {
                await this.RemoveDirectly(key);
            }

            // Stryker restore all
        }

        logger.LogInformation("Cache MISS on: {Key}", key);
        var value = await factory();
        await this.SetDirectly(key, value, expiry);
        return value;
    }

    /// <inheritdoc/>
    public Task<(bool found, T? value)> TryGetDirectly<T>(string key)
    {
        var exists = this.cache.TryGetValue(key, out var entry);

        // Stryker disable once equality
        if (exists && DateTimeOffset.UtcNow >= entry.Expiry)
        {
            this.RemoveDirectly(key);
            exists = false;
        }

        return Task.FromResult(exists ? (true, (T?)entry.Value) : (false, default));
    }

    /// <inheritdoc/>
    public Task<Dictionary<string, T>> TryGetManyDirectly<T>(params string[] keys)
    {
        var result = keys
            .Select(k => new { key = k, fetch = this.TryGetDirectly<T>(k).Result })
            .Where(k => k.fetch.found)
            .ToDictionary(k => k.key, k => k.fetch.value!);

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<bool> RemoveDirectly(string key)
    {
        var retVal = this.cache.TryRemove(key, out _);
        return Task.FromResult(retVal);
    }

    /// <inheritdoc/>
    public Task SetDirectly<T>(string key, T value, TimeSpan? expiry = null)
    {
        var expires = DateTimeOffset.UtcNow.Add(expiry ?? ICache.DefaultExpiry);
        this.cache[key] = (value, expires);
        return Task.CompletedTask;
    }
}
