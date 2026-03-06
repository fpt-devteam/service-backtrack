using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetPendingInvitations;

public sealed record GetPendingInvitationsQuery : IRequest<GetPendingInvitationsResult>
{
    public required Guid OrganizationId { get; init; }
    public string? UserId { get; init; }
}
