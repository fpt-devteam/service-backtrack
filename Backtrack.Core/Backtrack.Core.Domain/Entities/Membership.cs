using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class Membership : Entity<Guid>
{
    public required Guid OrganizationId { get; set; }
    public required string UserId { get; set; }
    public required MembershipRole Role { get; set; }
    public required MembershipStatus Status { get; set; } = MembershipStatus.Active;
    public required DateTimeOffset JoinedAt { get; set; }

    public Organization Organization { get; set; } = default!;
    public User User { get; set; } = default!;
}
