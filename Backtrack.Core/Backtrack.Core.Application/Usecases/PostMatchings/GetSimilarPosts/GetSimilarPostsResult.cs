using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed record GetSimilarPostsResult
{
    public IEnumerable<SimilarPostItem> SimilarPosts { get; init; } = Array.Empty<SimilarPostItem>();
}

public sealed record SimilarPostItem
{
    public required Guid Id { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required float MatchScore { get; init; }
    public required float DistanceMeters { get; init; }
}
