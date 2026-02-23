using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class Organization : Entity<Guid>
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Address { get; set; }
    public required string Phone { get; set; }
    public required string IndustryType { get; set; }
    public required string TaxIdentificationNumber { get; set; }
    public required OrganizationStatus Status { get; set; } = OrganizationStatus.Active;

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}
