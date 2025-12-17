using Backtrack.Core.Application.Posts.Common;

namespace Backtrack.Core.Application.Posts.Queries.SearchPostsBySemantic;

public sealed record PostSemanticSearchResult
{
    public required Guid Id { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public required string[] ImageUrls { get; init; }
    public LocationResult? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required double SimilarityScore { get; init; }
}
