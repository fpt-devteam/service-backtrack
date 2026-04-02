using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Posts.GetFeed;

public sealed record FeedPostResult
{
    public required Guid Id { get; init; }
    public PostAuthorResult? Author { get; init; }
    public OrganizationOnPost? Organization { get; init; }
    public required PostType PostType { get; init; }
    public required PostItem Item { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required double DistanceInMeters { get; init; }
}
