namespace Backtrack.Core.Application.Events;

public sealed class HandoverConfirmedIntegrationEvent
{
    public required Guid HandoverId { get; set; }
    public Guid? FinderPostId { get; set; }
    public Guid? OwnerPostId { get; set; }
    public required string FinderId { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
