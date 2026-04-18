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
    public PersonalBelongingDetailResult? PersonalBelongingDetail { get; init; }
    public CardDetailResult? CardDetail { get; init; }
    public ElectronicDetailResult? ElectronicDetail { get; init; }
    public OtherDetailResult? OtherDetail { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public FinderInfo? FinderInfo { get; init; }
}

public sealed record PersonalBelongingDetailResult
{
    public string? Color { get; init; }
    public string? Brand { get; init; }
    public string? Material { get; init; }
    public string? Size { get; init; }
    public string? Condition { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string? AiDescription { get; init; }
    public string? AdditionalDetails { get; init; }
}

public sealed record CardDetailResult
{
    public string? CardNumberMasked { get; init; }
    public string? HolderName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public string? IssuingAuthority { get; init; }
    public string? AiDescription { get; init; }
}

public sealed record ElectronicDetailResult
{
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? Color { get; init; }
    public bool? HasCase { get; init; }
    public string? CaseDescription { get; init; }
    public string? ScreenCondition { get; init; }
    public string? LockScreenDescription { get; init; }
    public string? DistinguishingFeatures { get; init; }
    public string? AiDescription { get; init; }
    public string? AdditionalDetails { get; init; }
}

public sealed record OtherDetailResult
{
    public required string ItemIdentifier { get; init; }
    public string? PrimaryColor { get; init; }
    public string? Notes { get; init; }
    public string? AiDescription { get; init; }
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
            PersonalBelongingDetail = post.PersonalBelongingDetail is { } pb ? new PersonalBelongingDetailResult
            {
                Color = pb.Color,
                Brand = pb.Brand,
                Material = pb.Material,
                Size = pb.Size,
                Condition = pb.Condition,
                DistinctiveMarks = pb.DistinctiveMarks,
                AiDescription = pb.AiDescription,
                AdditionalDetails = pb.AdditionalDetails
            } : null,
            CardDetail = post.CardDetail is { } cd ? new CardDetailResult
            {
                CardNumberMasked = cd.CardNumberMasked,
                HolderName = cd.HolderName,
                DateOfBirth = cd.DateOfBirth,
                IssueDate = cd.IssueDate,
                ExpiryDate = cd.ExpiryDate,
                IssuingAuthority = cd.IssuingAuthority,
                AiDescription = cd.AiDescription
            } : null,
            ElectronicDetail = post.ElectronicDetail is { } ed ? new ElectronicDetailResult
            {
                Brand = ed.Brand,
                Model = ed.Model,
                Color = ed.Color,
                HasCase = ed.HasCase,
                CaseDescription = ed.CaseDescription,
                ScreenCondition = ed.ScreenCondition,
                LockScreenDescription = ed.LockScreenDescription,
                DistinguishingFeatures = ed.DistinguishingFeatures,
                AiDescription = ed.AiDescription,
                AdditionalDetails = ed.AdditionalDetails
            } : null,
            OtherDetail = post.OtherDetail is { } od ? new OtherDetailResult
            {
                ItemIdentifier = od.ItemIdentifier,
                PrimaryColor = od.PrimaryColor,
                Notes = od.Notes,
                AiDescription = od.AiDescription
            } : null,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
