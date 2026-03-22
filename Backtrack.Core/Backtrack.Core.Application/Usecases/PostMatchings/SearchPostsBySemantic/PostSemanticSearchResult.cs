using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostMatchings.SearchPostsBySemantic;

public sealed record PostSemanticSearchResult
{
    public required Guid Id { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public List<PostImageResult> Images { get; init; } = new();
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required double SimilarityScore { get; init; }
}
