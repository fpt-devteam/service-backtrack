using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Queries.GetPosts;

public sealed record GetPostsQuery : IRequest<PagedResult<PostResult>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? PostType { get; init; }
    public string? AuthorId { get; init; }
    public string? SearchTerm { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusInKm { get; init; }
}
