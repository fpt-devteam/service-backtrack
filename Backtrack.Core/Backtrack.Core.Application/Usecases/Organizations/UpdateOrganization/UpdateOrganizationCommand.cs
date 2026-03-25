using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateOrganization;

public sealed record UpdateOrganizationCommand : IRequest<OrganizationResult>
{
    public Guid OrgId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public GeoPoint? Location { get; init; }
    public string? DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? Phone { get; init; }
    public string? ContactEmail { get; init; }
    public string? IndustryType { get; init; }
    public string? TaxIdentificationNumber { get; init; }
    public string? LogoUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? LocationNote { get; init; }
    public List<DailySchedule>? BusinessHours { get; init; }
}
