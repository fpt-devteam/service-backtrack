using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class JoinInvitation : Entity<Guid>
{
    public required Guid OrganizationId { get; set; }
    public required string Email { get; set; }
    public required MembershipRole Role { get; set; }
    public required string HashCode { get; set; }
    public required DateTimeOffset ExpiredTime { get; set; }
    public required InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public required string InvitedBy { get; set; }

    public Organization Organization { get; set; } = default!;
    public User InviterUser { get; set; } = default!;
}
