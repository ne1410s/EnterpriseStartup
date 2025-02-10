// <copyright file="PageExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Utils.Pagination;

using System;
using System.Linq;
using System.Linq.Expressions;

/// <summary>
/// Extensions for paging.
/// </summary>
public static class PageExtensions
{
    /// <summary>
    /// Returns page data for a source of items, without calculating the total
    /// number of records and pages.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="source">The item source.</param>
    /// <param name="request">The paging request.</param>
    /// <param name="where">Optional filter criteria.</param>
    /// <returns>A paging result.</returns>
    public static LazyPageResult<T> PageLazily<T>(
        this IQueryable<T> source,
        PageRequest request,
        Expression<Func<T, bool>>? where = null)
    {
        request ??= new PageRequest();
        var usedPage = Math.Clamp(request.PageNumber, 1, int.MaxValue);
        var usedSize = Math.Clamp(request.PageSize, 1, 1000);
        if (where != null)
        {
            source = source.Where(where);
        }

        // Stryker disable once linq
        var data = source
            .OrderBy(e => 1)
            .Skip((usedPage - 1) * usedSize)
            .Take(usedSize)
            .ToList();

        return new()
        {
            Data = data,
            PageSize = usedSize,
            PageNumber = request.PageNumber,
        };
    }

    /// <summary>
    /// Returns page data for a source of items, including the total item count.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="source">The item source.</param>
    /// <param name="request">The paging request.</param>
    /// <param name="where">Optional filter criteria.</param>
    /// <returns>A paging result.</returns>
    public static PageResult<T> Page<T>(
        this IQueryable<T> source,
        PageRequest request,
        Expression<Func<T, bool>>? where = null)
    {
        var lazyResult = source.PageLazily(request, where);
        var total = source.Count();
        return new()
        {
            Data = lazyResult.Data,
            PageSize = lazyResult.PageSize,
            PageNumber = lazyResult.PageNumber,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling((double)total / lazyResult.PageSize),
        };
    }
}
