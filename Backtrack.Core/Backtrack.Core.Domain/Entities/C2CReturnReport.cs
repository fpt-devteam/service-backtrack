using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class C2CReturnReport : Entity<Guid>
{
    public string FinderId { get; set; } = default!;
    public string OwnerId { get; set; } = default!;
    public required ReturnReportStatus Status { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public Guid? FinderPostId { get; set; }
    public Guid? OwnerPostId { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }

    public User? Finder { get; set; }
    public User? Owner { get; set; }
    public Post? FinderPost { get; set; }
    public Post? OwnerPost { get; set; }
}