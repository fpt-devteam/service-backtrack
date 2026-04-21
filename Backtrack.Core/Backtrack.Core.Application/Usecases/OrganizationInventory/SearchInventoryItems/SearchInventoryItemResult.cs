using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;

public sealed record InventoryItemResult
{
    public required Guid Id { get; init; }
    public PostAuthorResult? Author { get; init; }
    public InventoryOrganizationResult? Organization { get; init; }
    public required PostType PostType { get; init; }
    public required PostStatus Status { get; init; }
    public required string PostTitle { get; init; }
    public required ItemCategory Category { get; init; }
    public required Guid SubcategoryId { get; init; }
    public PersonalBelongingDetailDto? PersonalBelongingDetail { get; init; }
    public CardDetailDto? CardDetail { get; init; }
    public ElectronicDetailDto? ElectronicDetail { get; init; }
    public OtherDetailDto? OtherDetail { get; init; }
    public List<string> ImageUrls { get; init; } = [];
    public required string InternalLocation { get; init; }
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required FinderInfo FinderInfo { get; init; }
    public OwnerInfo? OwnerInfo { get; init; }
    public DateTimeOffset? ReturnReportExpiresAt { get; init; }
}

public static class InventoryItemResultMapper
{
    public static InventoryItemResult ToInventoryItemResult(
        this Post post,
        OrgReceiveReport? receiveReport = null,
        OrgReturnReport? returnReport = null)
    {
        return new InventoryItemResult
        {
            Id                      = post.Id,
            Author                  = post.Author?.ToPostAuthorResult(),
            Organization            = post.Organization?.ToInventoryOrganizationResult(),
            PostType                = post.PostType,
            Status                  = post.Status,
            PostTitle               = post.PostTitle,
            Category                = post.Category,
            SubcategoryId           = post.SubcategoryId,
            PersonalBelongingDetail = post.PersonalBelongingDetail?.ToDto(),
            CardDetail              = post.CardDetail?.ToDto(),
            ElectronicDetail        = post.ElectronicDetail?.ToDto(),
            OtherDetail             = post.OtherDetail?.ToDto(),
            ImageUrls               = post.ImageUrls,
            InternalLocation        = post.InternalLocation ?? string.Empty,
            Location                = post.Location!,
            ExternalPlaceId         = post.ExternalPlaceId,
            DisplayAddress          = post.DisplayAddress,
            EventTime               = post.EventTime,
            CreatedAt               = post.CreatedAt,
            FinderInfo              = receiveReport?.FinderInfo ?? new FinderInfo(),
            OwnerInfo               = returnReport?.OwnerInfo,
            ReturnReportExpiresAt   = returnReport?.ExpiresAt
        };
    }
}
