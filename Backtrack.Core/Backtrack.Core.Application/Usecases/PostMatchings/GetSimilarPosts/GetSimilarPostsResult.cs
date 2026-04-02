using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed record GetSimilarPostsResult
{
    public IEnumerable<SimilarPostItem> SimilarPosts { get; init; } = Array.Empty<SimilarPostItem>();
}

/// <summary>
/// A single criterion combining the pre-computed 0-100 score with LLM reasoning points.
/// Score is always present (computed from embeddings/formulas). Points are null when LLM has not run yet.
/// </summary>
public sealed record CriterionResult
{
    public required int Score { get; init; }
    public CriterionPoint[]? Points { get; init; }
}

/// <summary>All four match criteria for a similar post.</summary>
public sealed record SimilarPostCriteria
{
    public required CriterionResult VisualAnalysis { get; init; }
    public required CriterionResult Description { get; init; }
    public required CriterionResult Location { get; init; }
    public required CriterionResult TimeWindow { get; init; }
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
    public required MatchingLevel MatchingLevel { get; init; }
    public bool IsAssessed { get; init; }
    public string? AssessmentSummary { get; init; }
    public SimilarPostCriteria? Criteria { get; init; }
}
