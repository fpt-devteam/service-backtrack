namespace Backtrack.Core.Domain.ValueObjects;

public sealed record FinderInfo
{
    public string? FinderName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? NationalId { get; init; }
    public string? OrgMemberId { get; init; }
}
