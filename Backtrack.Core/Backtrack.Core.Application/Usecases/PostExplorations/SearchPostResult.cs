using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostExplorations;

public sealed record SearchPostResult
{
    public required Guid Id { get; init; }
    public PostAuthorResult? Author { get; init; }
    public OrganizationOnPost? Organization { get; init; }
    public required string PostTitle { get; init; }
    public required PostType PostType { get; init; }
    public required ItemCategory Category { get; init; }
    public required Guid SubcategoryId { get; init; }
    public PersonalBelongingDetailDto? PersonalBelongingDetail { get; init; }
    public CardDetailDto? CardDetail { get; init; }
    public ElectronicDetailDto? ElectronicDetail { get; init; }
    public OtherDetailDto? OtherDetail { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public double? Score { get; init; }
    public double? DistanceInMeters { get; init; }
}
