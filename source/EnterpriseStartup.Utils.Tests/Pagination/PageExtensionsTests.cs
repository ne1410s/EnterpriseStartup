// <copyright file="PageExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace EnterpriseStartup.Utils.Tests.Pagination;

using EnterpriseStartup.Utils.Pagination;

/// <summary>
/// Tests for the <see cref="PageExtensions"/> class.
/// </summary>
public class PageExtensionsTests
{
    [Fact]
    public void PageLazily_WhenCalled_ReturnsExpected()
    {
        // Arrange
        var source = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var request = new PageRequest(PageNumber: 3, PageSize: 2);
        var expected = new LazyPageResult<int>
        {
            Data = [9],
            PageNumber = 3,
            PageSize = 2,
        };

        // Act
        var result = source.PageLazily(request, n => n % 2 != 0);

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void Page_WhenCalled_ReturnsExpected()
    {
        // Arrange
        var source = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var request = new PageRequest(PageSize: 3);
        var expected = new PageResult<int>
        {
            Data = [2, 4, 6],
            PageNumber = 1,
            PageSize = 3,
            TotalPages = 3,
            TotalRecords = 9,
        };

        // Act
        var result = source.Page(request, n => n % 2 == 0);

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }
}