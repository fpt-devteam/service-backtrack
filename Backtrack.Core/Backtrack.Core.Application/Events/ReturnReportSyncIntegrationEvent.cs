namespace Backtrack.Core.Application.Events;

public sealed class ReturnReportSyncIntegrationEvent
{
    public required Guid C2CReturnReportId { get; set; }

    // Participants
    public required string FinderId { get; set; }
    public string? FinderDisplayName { get; set; }
    public string? FinderAvatarUrl { get; set; }
    public string? FinderEmail { get; set; }

    public required string OwnerId { get; set; }
    public string? OwnerDisplayName { get; set; }
    public string? OwnerAvatarUrl { get; set; }
    public string? OwnerEmail { get; set; }

    // Post references
    public Guid? FinderPostId { get; set; }
    public string? FinderPostType { get; set; }
    public Guid? OwnerPostId { get; set; }
    public string? OwnerPostType { get; set; }

    // Status
    public required string Status { get; set; }
    public string? ActivatedByRole { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
