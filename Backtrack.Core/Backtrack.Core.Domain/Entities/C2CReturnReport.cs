using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class C2CReturnReport : Entity<Guid>
{
    public required string FinderId { get; set; }
    public required string OwnerId { get; set; }
    public required ReturnReportStatus Status { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public Guid? FinderPostId { get; set; }
    public Guid? OwnerPostId { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }

    public User Finder { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public Post? FinderPost { get; set; }
    public Post? OwnerPost { get; set; }
}