namespace Backtrack.Core.Application.Usecases.Posts.Queries.GetSimilarPosts;

/// <summary>
/// Result for similar posts query
/// </summary>
public sealed record GetSimilarPostsResult
{
    /// <summary>
    /// Status of the embedding for the queried post
    /// </summary>
    public required string EmbeddingStatus { get; init; }

    /// <summary>
    /// Whether the embedding is ready (Ready status)
    /// </summary>
    public required bool IsReady { get; init; }

    /// <summary>
    /// List of similar posts (only populated when IsReady = true)
    /// </summary>
    public IEnumerable<SimilarPostItem> SimilarPosts { get; init; } = Array.Empty<SimilarPostItem>();
}

/// <summary>
/// Represents a similar post item with similarity score
/// </summary>
public sealed record SimilarPostItem
{
    public required Guid Id { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public LocationResult? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Similarity score (0.0 to 1.0, higher is more similar)
    /// </summary>
    public required double SimilarityScore { get; init; }
}
