using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class Handover : Entity<Guid>
{
    public required HandoverType Type { get; set; }
    public Guid? FinderPostId { get; set; }
    public Guid? OwnerPostId { get; set; }
    public required HandoverStatus Status { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }

    // Navigation properties
    public Post? FinderPost { get; set; }
    public Post? OwnerPost { get; set; }
    public HandoverOrgExtension? OrgExtension { get; set; }
}
