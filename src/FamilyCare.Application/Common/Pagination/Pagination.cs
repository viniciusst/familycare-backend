namespace FamilyCare.Application.Common.Pagination;

/// <summary>Standard pagination request (skip/take based).</summary>
public sealed record PagedRequest(int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 20,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
    public int Take => NormalizedPageSize;
}

/// <summary>Standard pagination response.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>Helpers for building <see cref="PagedResult{T}"/>.</summary>
public static class PagedResult
{
    public static PagedResult<T> Empty<T>(int page, int pageSize)
        => new([], page, pageSize, 0);
}
