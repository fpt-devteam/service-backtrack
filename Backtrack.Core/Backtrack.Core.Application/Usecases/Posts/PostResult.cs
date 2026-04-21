using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record PostResult
{
    public required Guid Id { get; init; }
    public PostAuthorResult? Author { get; init; }
    public required PostType PostType { get; init; }
    public required PostStatus Status { get; init; }
    public required ItemCategory Category { get; init; }
    public required Guid SubcategoryId { get; init; }
    public required string PostTitle { get; init; }
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
}

public static class PostResultMapper
{
    public static PostResult ToPostResult(this Post post)
    {
        return new PostResult
        {
            Id = post.Id,
            Author = post.Author?.ToPostAuthorResult(),
            PostType = post.PostType,
            Status = post.Status,
            Category = post.Category,
            SubcategoryId = post.SubcategoryId,
            PostTitle = post.PostTitle,
            PersonalBelongingDetail = post.PersonalBelongingDetail?.ToDto(),
            CardDetail              = post.CardDetail?.ToDto(),
            ElectronicDetail        = post.ElectronicDetail?.ToDto(),
            OtherDetail             = post.OtherDetail?.ToDto(),
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
