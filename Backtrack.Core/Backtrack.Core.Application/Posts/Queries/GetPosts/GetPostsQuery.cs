using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.GetPosts;

public sealed record GetPostsQuery : IRequest<PagedResult<PostResult>>
{
    public required PagedQuery PagedQuery { get; init; }
    public PostType? PostType { get; init; }
    public string? AuthorId { get; init; }
    public string? SearchTerm { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusInKm { get; init; }
}
