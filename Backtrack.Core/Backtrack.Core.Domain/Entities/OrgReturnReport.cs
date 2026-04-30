using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;
public sealed class OrgReturnReport : Entity<Guid>
{
    public required Guid OrgId { get; set; }
    public required string StaffId { get; set; } = default!;
    public required Guid PostId { get; set; }
    public required List<string> EvidenceImageUrls { get; set; }
    public required OwnerInfo OwnerInfo { get; set; }

    public Organization Organization { get; set; } = default!;
    public User Staff { get; set; } = default!;
    public Post Post { get; set; } = default!;
}
