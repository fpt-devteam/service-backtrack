namespace Backtrack.Core.Application.Events;

public sealed class OrgEnsureExistIntegrationEvent
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string LogoUrl { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
