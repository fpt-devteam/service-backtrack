using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public abstract class Handover : Entity<Guid>
{
    public required HandoverStatus Status { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
}

public sealed class P2PHandover : Handover
{
    public required string FinderId { get; set; }
    public required string OwnerId { get; set; }
    public Guid? FinderPostId { get; set; }
    public Guid? OwnerPostId { get; set; }

    public User Finder { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public Post? FinderPost { get; set; }
    public Post? OwnerPost { get; set; }
}

public sealed class OrgHandover : Handover
{
    public required string FinderId { get; set; }
    public required Guid OrgId { get; set; }
    public required string StaffId { get; set; }
    public Guid? FinderPostId { get; set; }

    public bool OwnerVerified { get; set; }
    public Dictionary<string, string>? OwnerFormData { get; set; }
    public DateTimeOffset? StaffConfirmedAt { get; set; }
    public DateTimeOffset? OwnerConfirmedAt { get; set; }

    public User Finder { get; set; } = default!;
    public Organization Organization { get; set; } = default!;
    public User Staff { get; set; } = default!;
    public Post? FinderPost { get; set; }
}
