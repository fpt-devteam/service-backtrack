using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record OrganizationOnPost
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public GeoPoint? Location { get; init; }
    public string? DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public required string Phone { get; init; }
    public required string IndustryType { get; init; }
    public required string LogoUrl { get; set; }
}

public static class OrganizationOnPostMapper
{
    public static OrganizationOnPost ToOrganizationOnPost(this Organization organization)
    {
        return new OrganizationOnPost
        {
            Id = organization.Id,
            Name = organization.Name,
            Slug = organization.Slug,
            Location = organization.Location,
            DisplayAddress = organization.DisplayAddress,
            ExternalPlaceId = organization.ExternalPlaceId,
            Phone = organization.Phone,
            IndustryType = organization.IndustryType,
            LogoUrl = organization.LogoUrl
        };
    }
}