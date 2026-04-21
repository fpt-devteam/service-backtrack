using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory;

public sealed record InventoryOrganizationResult
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

public static class InventoryOrganizationResultMapper
{
    public static InventoryOrganizationResult ToInventoryOrganizationResult(this Organization organization)
    {
        return new InventoryOrganizationResult
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
