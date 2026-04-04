using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Organizations.GetMyOrganizations;

public sealed record MyOrganizationResult
{
    public required Guid OrgId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required GeoPoint Location { get; init; }
    public required string DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public required string Phone { get; init; }
    public string? ContactEmail { get; init; }
    public required string IndustryType { get; init; }
    public required string TaxIdentificationNumber { get; init; }
    public required string LogoUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? LocationNote { get; init; }
    public List<DailySchedule>? BusinessHours { get; init; }
    public required List<FinderContactField> RequiredFinderContactFields { get; init; }
    public required List<FormFieldDefinition> RequiredOwnerFormFields { get; init; }
    public required string OrgStatus { get; init; }
    public required string MyRole { get; init; }
    public required DateTimeOffset JoinedAt { get; init; }
}
