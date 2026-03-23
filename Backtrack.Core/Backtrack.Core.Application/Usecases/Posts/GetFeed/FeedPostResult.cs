using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Posts.GetFeed;

public sealed record FeedPostResult
{
    public required Guid Id { get; init; }
    public AuthorResult? Author { get; init; }
    public OrganizationOnPost? Organization { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public List<PostImageResult> Images { get; init; } = new();
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required double DistanceInMeters { get; init; }
}
