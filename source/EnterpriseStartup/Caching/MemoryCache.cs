// <copyright file="MemoryCache.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Caching;

using System;
using System.Collections.Concurrent;
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
            if (DateTimeOffset.UtcNow < cacheEntry.Expiry)
            {
                logger.LogInformation("Cache HIT on: {Key}", key);
                return (T?)cacheEntry.Value;
            }
            else
            {
                await this.RemoveDirectly(key);
            }
        }

        logger.LogInformation("Cache MISS on: {Key}", key);
        var value = await factory();
        await this.SetDirectly(key, value, expiry);
        return value;
    }

    /// <inheritdoc/>
    public async Task<(bool found, T? value)> TryGetDirectly<T>(string key)
    {
        await Task.CompletedTask;
        return !this.cache.TryGetValue(key, out var entry)
            ? (false, default)
            : (true, (T?)entry.Value);
    }

    /// <inheritdoc/>
    public Task RemoveDirectly(string key)
    {
        this.cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetDirectly<T>(string key, T value, TimeSpan? expiry = null)
    {
        var expires = DateTimeOffset.UtcNow.Add(expiry ?? ICache.DefaultExpiry);
        this.cache[key] = (value, expires);
        return Task.CompletedTask;
    }
}
