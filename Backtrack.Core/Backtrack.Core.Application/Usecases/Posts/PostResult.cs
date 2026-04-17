using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record PostResult
{
    public required Guid Id { get; init; }
    public PostAuthorResult? Author { get; init; }
    public OrganizationOnPost? Organization { get; init; }
    public required PostType PostType { get; init; }
    public required PostStatus Status { get; init; }
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
    public FinderInfo? FinderInfo { get; init; }
}

public static class PostResultMapper
{
    public static PostResult ToPostResult(this Post post)
    {
        return new PostResult
        {
            Id = post.Id,
            Author = post.Author?.ToPostAuthorResult(),
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType,
            Status = post.Status,
            Category = post.Category,
            SubcategoryId = post.SubcategoryId,
            PersonalBelongingDetail = post.PersonalBelongingDetail,
            CardDetail = post.CardDetail,
            ElectronicDetail = post.ElectronicDetail,
            OtherDetail = post.OtherDetail,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
