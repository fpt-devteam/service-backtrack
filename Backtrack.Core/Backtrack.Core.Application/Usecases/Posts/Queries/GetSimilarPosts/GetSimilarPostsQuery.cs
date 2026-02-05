using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Queries.GetSimilarPosts;

/// <summary>
/// Query to get similar posts based on vector similarity and geographic proximity
/// </summary>
public sealed record GetSimilarPostsQuery : IRequest<GetSimilarPostsResult>
{
    /// <summary>
    /// ID of the post to find similar items for
    /// </summary>
    public required Guid PostId { get; init; }

    /// <summary>
    /// Maximum number of similar posts to return (default: 20)
    /// </summary>
    public int Limit { get; init; } = 20;
}
