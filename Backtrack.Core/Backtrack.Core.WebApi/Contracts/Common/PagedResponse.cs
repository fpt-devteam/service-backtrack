namespace Backtrack.Core.WebApi.Contracts.Common;

public record PagedResponse<T>
{
    public required IEnumerable<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }

    public static PagedResponse<T> Create(
        IEnumerable<T> items,
        int page,
        int pageSize,
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
