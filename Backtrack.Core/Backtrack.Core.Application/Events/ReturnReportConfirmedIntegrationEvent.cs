namespace Backtrack.Core.Application.Events;

public sealed class ReturnReportConfirmedIntegrationEvent
{
    public required Guid C2CReturnReportId { get; set; }
    public Guid? FinderPostId { get; set; }
    public Guid? OwnerPostId { get; set; }
    public required string FinderId { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
