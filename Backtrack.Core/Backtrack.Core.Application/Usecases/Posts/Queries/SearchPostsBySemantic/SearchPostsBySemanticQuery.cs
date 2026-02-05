using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Queries.SearchPostsBySemantic;

public sealed record SearchPostsBySemanticQuery : IRequest<PagedResult<PostSemanticSearchResult>>
{
    public required string SearchText { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? PostType { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusInKm { get; init; }
}
