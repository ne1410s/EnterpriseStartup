// <copyright file="PageResult.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Utils.Pagination;

using System.Collections.Generic;

/// <summary>
/// A page result where the total number of records is not determined.
/// </summary>
/// <typeparam name="T">The paged item type.</typeparam>
public record LazyPageResult<T>
{
    /// <summary>
    /// Gets the page number.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the particular page.
    /// </summary>
    public ICollection<T> Data { get; init; } = [];
}

/// <summary>
/// A page result where the total number of records is determined.
/// </summary>
/// <typeparam name="T">The paged item type.</typeparam>
public record PageResult<T> : LazyPageResult<T>
{
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Gets the total number of records.
    /// </summary>
    public int TotalRecords { get; init; }
}