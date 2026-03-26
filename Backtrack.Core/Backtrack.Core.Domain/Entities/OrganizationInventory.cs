using Backtrack.Core.Domain.Constants;
using System;

namespace Backtrack.Core.Domain.Entities;

public sealed class OrganizationInventory : Entity<Guid>
{
    public required Guid OrgId { get; set; }
    public required string LoggedById { get; set; }
    public required string ItemName { get; set; }
    public required string Description { get; set; }
    public string? DistinctiveMarks { get; set; }
    public string[] ImageUrls { get; set; } = Array.Empty<string>();
    public string? StorageLocation { get; set; }
    public float[]? MultimodalEmbedding { get; set; }
    public required OrganizationInventoryStatus Status { get; set; }
    public required DateTimeOffset LoggedAt { get; set; }
    public required string ReceiverStaffId { get; set; }
    public string? HandoverStaffId { get; set; }

    public Organization Organization { get; set; } = default!;
    public User LoggedBy { get; set; } = default!;
    public User ReceiverStaff { get; set; } = default!;
    public User? HandoverStaff { get; set; }
    public FinderContact? FinderContact { get; set; }
}
