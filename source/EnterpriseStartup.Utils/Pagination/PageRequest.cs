// <copyright file="PageRequest.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Utils.Pagination;

/// <summary>
/// A page request.
/// </summary>
/// <param name="PageNumber">The page number.</param>
/// <param name="PageSize">The page size.</param>
public record PageRequest(int PageNumber = 1, int PageSize = 100);
