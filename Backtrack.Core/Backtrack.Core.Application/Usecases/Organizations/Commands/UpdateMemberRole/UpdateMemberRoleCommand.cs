using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.UpdateMemberRole;

public sealed record UpdateMemberRoleCommand : IRequest<MemberResult>
{
    public Guid OrgId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public Guid TargetMembershipId { get; init; }
    public required string Role { get; init; }
}
