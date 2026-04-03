using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostExplorations.GetFeed;

public sealed record FeedPostItem
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

public sealed record FeedPostResult
{
    public List<FeedPostItem> Electronics { get; init; } = [];
    public List<FeedPostItem> Clothing { get; init; } = [];
    public List<FeedPostItem> Accessories { get; init; } = [];
    public List<FeedPostItem> Documents { get; init; } = [];
    public List<FeedPostItem> Wallet { get; init; } = [];
    public List<FeedPostItem> Suitcase { get; init; } = [];
    public List<FeedPostItem> Bags { get; init; } = [];
    public List<FeedPostItem> Keys { get; init; } = [];
    public List<FeedPostItem> Other { get; init; } = [];
}
