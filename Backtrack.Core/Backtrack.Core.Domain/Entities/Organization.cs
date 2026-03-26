using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class Organization : Entity<Guid>
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required GeoPoint Location { get; set; }
    public required string DisplayAddress { get; set; }
    public string? ExternalPlaceId { get; set; }
    public required string Phone { get; set; }
    public string? ContactEmail { get; set; }
    public required string IndustryType { get; set; }
    public required string TaxIdentificationNumber { get; set; }
    public required string LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? LocationNote { get; set; }
    public List<DailySchedule>? BusinessHours { get; set; }
    public required List<FinderContactField> RequiredFinderContactFields { get; set; }
    public required OrganizationStatus Status { get; set; } = OrganizationStatus.Active;

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
