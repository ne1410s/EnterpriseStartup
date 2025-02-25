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
    /// <param name="orderBy">Optional order criteria.</param>
    /// <param name="descending">Whether to order descending.</param>
    /// <returns>A paging result.</returns>
    public static LazyPageResult<T> PageLazily<T>(
        this IQueryable<T> source,
        PageRequest request,
        Expression<Func<T, bool>>? where = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false)
    {
        request ??= new PageRequest();
        var usedPage = Math.Clamp(request.PageNumber, 1, int.MaxValue);
        var usedSize = Math.Clamp(request.PageSize, 1, 1000);

        if (where != null)
        {
            source = source.Where(where);
        }

        // Stryker disable all
        if (orderBy != null)
        {
            source = descending ? source.OrderByDescending(orderBy) : source.OrderBy(orderBy);
        }
        else
        {
            source = source.OrderBy(e => 1); // Default ordering
        }

        // Stryker restore all
        var data = source
            .Skip((usedPage - 1) * usedSize)
            .Take(usedSize + 1)
            .ToList();

        return new()
        {
            Data = [.. data.Take(usedSize)],
            PageSize = usedSize,
            PageNumber = request.PageNumber,
            HasMore = data.Count > usedSize,
        };
    }

    /// <summary>
    /// Returns page data for a source of items, including the total item count.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="source">The item source.</param>
    /// <param name="request">The paging request.</param>
    /// <param name="where">Optional filter criteria.</param>
    /// <param name="orderBy">Optional order criteria.</param>
    /// <param name="descending">Whether to order descending.</param>
    /// <returns>A paging result.</returns>
    public static PageResult<T> Page<T>(
        this IQueryable<T> source,
        PageRequest request,
        Expression<Func<T, bool>>? where = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false)
    {
        var lazyResult = source.PageLazily(request, where, orderBy, descending);
        if (where != null)
        {
            source = source.Where(where);
        }

        var total = source.Count();
        return new()
        {
            Data = lazyResult.Data,
            PageSize = lazyResult.PageSize,
            PageNumber = lazyResult.PageNumber,
            TotalRecords = total,
            TotalPages = Math.Max(1, (int)Math.Ceiling((double)total / lazyResult.PageSize)),
        };
    }

    /// <summary>
    /// Maps a lazy page result into another type, post-execution.
    /// </summary>
    /// <typeparam name="TIn">The source paged item type.</typeparam>
    /// <typeparam name="TOut">The target paged item type.</typeparam>
    /// <param name="source">The source paging.</param>
    /// <param name="mapper">The function to convert an item.</param>
    /// <returns>Mapped result.</returns>
    public static LazyPageResult<TOut> MapFinal<TIn, TOut>(
        this LazyPageResult<TIn> source,
        Func<TIn, TOut> mapper)
    {
        source = source ?? throw new ArgumentNullException(nameof(source));
        return new LazyPageResult<TOut>
        {
            PageNumber = source.PageNumber,
            PageSize = source.PageSize,
            Data = [.. source.Data.Select(mapper)],
            HasMore = source.HasMore,
        };
    }

    /// <summary>
    /// Maps a page result into another type, post-execution.
    /// </summary>
    /// <typeparam name="TIn">The source paged item type.</typeparam>
    /// <typeparam name="TOut">The target paged item type.</typeparam>
    /// <param name="source">The source paging.</param>
    /// <param name="mapper">The function to convert an item.</param>
    /// <returns>Mapped result.</returns>
    public static PageResult<TOut> MapFinal<TIn, TOut>(
        this PageResult<TIn> source,
        Func<TIn, TOut> mapper)
    {
        source = source ?? throw new ArgumentNullException(nameof(source));
        return new PageResult<TOut>
        {
            PageNumber = source.PageNumber,
            PageSize = source.PageSize,
            Data = [.. source.Data.Select(mapper)],
            HasMore = source.HasMore,
            TotalPages = source.TotalPages,
            TotalRecords = source.TotalRecords,
        };
    }
}
