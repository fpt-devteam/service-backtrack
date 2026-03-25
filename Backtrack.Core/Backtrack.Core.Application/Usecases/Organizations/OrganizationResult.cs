using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record OrganizationResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public GeoPoint? Location { get; init; }
    public string? DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public required string Phone { get; init; }
    public string? ContactEmail { get; init; }
    public required string IndustryType { get; init; }
    public required string TaxIdentificationNumber { get; init; }
    public required string LogoUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? LocationNote { get; init; }
    public List<DailySchedule>? BusinessHours { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
