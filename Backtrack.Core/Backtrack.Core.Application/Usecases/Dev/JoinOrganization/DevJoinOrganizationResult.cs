namespace Backtrack.Core.Application.Usecases.Dev.JoinOrganization;

public sealed record DevJoinOrganizationResult
{
    public required Guid MembershipId { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string UserId { get; init; }
    public required string Role { get; init; }
}
