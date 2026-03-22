using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.SearchPosts;

public sealed record SearchPostsCommand : IRequest<PagedResult<SearchPostResult>>
{
    public string? Query { get; init; }
    public SearchMode Mode { get; init; } = SearchMode.Keyword;
    public SearchFilters? Filters { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record SearchFilters
{
    public string? PostType { get; init; }
    public double? RadiusInKm { get; init; }
    public GeoPoint? Location { get; init; }
    public Guid? OrganizationId { get; init; }
}
