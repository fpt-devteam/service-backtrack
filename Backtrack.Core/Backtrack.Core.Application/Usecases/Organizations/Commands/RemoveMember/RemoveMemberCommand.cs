using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.RemoveMember;

public sealed record RemoveMemberCommand : IRequest<Unit>
{
    public Guid OrgId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public required Guid TargetMembershipId { get; init; }
}
