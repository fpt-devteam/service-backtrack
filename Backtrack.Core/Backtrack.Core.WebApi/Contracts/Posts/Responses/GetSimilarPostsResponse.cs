using Backtrack.Core.WebApi.Contracts.Common;

namespace Backtrack.Core.WebApi.Contracts.Posts.Responses;

/// <summary>
/// Response for getting similar posts
/// </summary>
public sealed record GetSimilarPostsResponse
{
    /// <summary>
    /// Status of the embedding for the queried post
    /// </summary>
    public required string EmbeddingStatus { get; init; }

    /// <summary>
    /// Whether the embedding is ready
    /// </summary>
    public required bool IsReady { get; init; }

    /// <summary>
    /// List of similar posts (only when IsReady = true)
    /// </summary>
    public IEnumerable<SimilarPostResponse> SimilarPosts { get; init; } = Array.Empty<SimilarPostResponse>();
}

/// <summary>
/// Similar post item with similarity score
/// </summary>
public sealed record SimilarPostResponse
{
    public required Guid Id { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public LocationResponse? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Similarity score (0.0 to 1.0, higher is more similar)
    /// </summary>
    public required double SimilarityScore { get; init; }
}
