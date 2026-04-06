using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;
public sealed class OrgReturnReport : Entity<Guid>
{
    public Guid OrgId { get; set; }
    public string StaffId { get; set; } = default!;
    public required DateTimeOffset ExpiresAt { get; set; }
    public required Guid PostId { get; set; }
    public OwnerInfo? OwnerInfo { get; set; }

    public Organization? Organization { get; set; }
    public User? Staff { get; set; }
    public Post? Post { get; set; }
}
