using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed record GetSimilarPostsResult
{
    public IEnumerable<SimilarPostItem> SimilarPosts { get; init; } = Array.Empty<SimilarPostItem>();
}

public sealed record SimilarPostItem
{
    public required Guid Id { get; init; }
    public required PostAuthorResult Author { get; init; }
    public required string PostTitle { get; init; }
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
    public required double Score { get; init; }
    public List<MatchEvidence> Evidence { get; init; } = [];
    public required MatchStatus Status { get; init; }
    // time gap and location distance
    public required TimeSpan TimeGap { get; init; }
    public required double LocationDistance { get; init; }
}
