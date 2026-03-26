namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record FinderContactInfo
{
    public required string Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? NationalId { get; init; }
    public string? OrgMemberId { get; init; }
}
