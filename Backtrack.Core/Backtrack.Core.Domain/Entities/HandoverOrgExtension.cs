namespace Backtrack.Core.Domain.Entities;

public sealed class HandoverOrgExtension : Entity<Guid>
{
    public required Guid HandoverId { get; set; }
    public required Guid OrgId { get; set; }
    public required string StaffId { get; set; }
    public bool OwnerVerified { get; set; }
    public Dictionary<string, string>? OwnerFormData { get; set; }
    public DateTimeOffset? StaffConfirmedAt { get; set; }
    public DateTimeOffset? OwnerConfirmedAt { get; set; }

    // Navigation properties
    public Handover Handover { get; set; } = default!;
    public Organization Organization { get; set; } = default!;
    public User Staff { get; set; } = default!;
}
