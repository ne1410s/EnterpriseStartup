// <copyright file="ICache.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Caching;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// A cache layer.
/// </summary>
public interface ICache
{
    /// <summary>
    /// Default cache expiry.
    /// </summary>
    public static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets a value, with a fallback function to populate and reset it.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="factory">The function to call on cache miss.</param>
    /// <param name="expiry">The expiry.</param>
    /// <returns>The value.</returns>
    public Task<T?> GetValue<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

    /// <summary>
    /// Gets a value direct from the store.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The key.</param>
    /// <returns>The return, include an indicator whether an entry was found.
    /// This helps determine whether anything was actually stored.</returns>
    public Task<(bool found, T? value)> TryGetDirectly<T>(string key);

    /// <summary>
    /// Gets a series of values from the store. The values are retrieved via a common type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="keys">The keys.</param>
    /// <returns>Cache hits.</returns>
    public Task<Dictionary<string, T>> TryGetManyDirectly<T>(params string[] keys);

    /// <summary>
    /// Sets a value direct to the store.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="expiry">The expiry.</param>
    /// <returns>Async task.</returns>
    public Task SetDirectly<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Removes a value directly from the store.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>True if entry was removed.</returns>
    public Task<bool> RemoveDirectly(string key);
}
