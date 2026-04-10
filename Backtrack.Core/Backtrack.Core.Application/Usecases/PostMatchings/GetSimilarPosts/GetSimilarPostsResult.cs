using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed record GetSimilarPostsResult
{
    public IEnumerable<SimilarPostItem> SimilarPosts { get; init; } = Array.Empty<SimilarPostItem>();
}

public sealed record SimilarPostItem
{
    public required Guid Id { get; init; }
    public required PostType PostType { get; init; }
    public required PostItem Item { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required float MatchScore { get; init; }
    public required float DistanceMeters { get; init; }
    public required double TimeGapDays { get; init; }
    public required MatchingLevel MatchingLevel { get; init; }
    public bool IsAssessed { get; init; }
    public string AssessmentSummary { get; init; } = string.Empty;
}
