using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class C2CReturnReport : Entity<Guid>
{
    public required string FinderId { get; set; } = default!;
    public required string OwnerId { get; set; } = default!;
    public required C2CReturnReportStatus Status { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public required Guid FinderPostId { get; set; }
    public required Guid OwnerPostId { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public List<string>? EvidenceImageUrls { get; set; }

    public User Finder { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public Post FinderPost { get; set; } = default!;
    public Post OwnerPost { get; set; } = default!;
}