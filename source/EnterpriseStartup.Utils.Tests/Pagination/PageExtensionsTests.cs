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
        var result = source.AsQueryable().PageLazily(request, n => n % 2 != 0);

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void PageLazily_PageSizeEqualsCount_HasMoreIsFalse()
    {
        // Arrange
        var source = new int[] { 1, 2, 3 };
        var request = new PageRequest(PageSize: source.Length);

        // Act
        var result = source.AsQueryable().PageLazily(request);

        // Assert
        result.HasMore.ShouldBeFalse();
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
            TotalPages = 2,
            TotalRecords = 4,
        };

        // Act
        var result = source.AsQueryable().Page(request, n => n % 2 == 0);

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void LazyPageResult_MapFinal_ReturnsExpected()
    {
        // Arrange
        var sut = new LazyPageResult<int>
        {
            PageNumber = 1,
            PageSize = 3,
            Data = [1, 2, 3],
            HasMore = true,
        };
        var expected = new LazyPageResult<bool>
        {
            PageNumber = 1,
            PageSize = 3,
            Data = [false, true, false],
            HasMore = true,
        };

        // Act
        var result = sut.MapFinal(x => x % 2 == 0);

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void PageResult_MapFinal_ReturnsExpected()
    {
        // Arrange
        var sut = new PageResult<int>
        {
            PageNumber = 1,
            PageSize = 3,
            Data = [1, 2, 3],
            HasMore = true,
            TotalPages = 10,
            TotalRecords = 30,
        };
        var expected = new PageResult<bool>
        {
            PageNumber = 1,
            PageSize = 3,
            Data = [false, true, false],
            HasMore = true,
            TotalPages = 10,
            TotalRecords = 30,
        };

        // Act
        var result = sut.MapFinal(x => x % 2 == 0);

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }
}