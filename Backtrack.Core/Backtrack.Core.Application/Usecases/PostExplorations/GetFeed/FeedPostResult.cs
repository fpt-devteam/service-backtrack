using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostExplorations.GetFeed;

public sealed record FeedPostItem
{
    public required Guid Id { get; init; }
    public PostAuthorResult? Author { get; init; }
    public OrganizationOnPost? Organization { get; init; }
    public required PostType PostType { get; init; }
    public required ItemCategory Category { get; init; }
    public required Guid SubcategoryId { get; init; }
    public PostPersonalBelongingDetail? PersonalBelongingDetail { get; init; }
    public PostCardDetail? CardDetail { get; init; }
    public PostElectronicDetail? ElectronicDetail { get; init; }
    public PostOtherDetail? OtherDetail { get; init; }
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
    public List<FeedPostItem> PersonalBelongings { get; init; } = [];
    public List<FeedPostItem> Cards { get; init; } = [];
    public List<FeedPostItem> Electronics { get; init; } = [];
    public List<FeedPostItem> Others { get; init; } = [];
}
