using System;
using System.Collections.Generic;

namespace RecyclingApp.Application.Common.Models;

/// <summary>
/// Standard pagination wrapper for query results.
/// </summary>
/// <typeparam name="T">Item type</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex < 1 ? 1 : pageIndex;
        PageSize = pageSize < 1 ? 10 : pageSize;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)PageSize);
        Items = items ?? Array.Empty<T>();
    }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int count, int pageIndex, int pageSize)
    {
        return new PagedResult<T>(items, count, pageIndex, pageSize);
    }
}
