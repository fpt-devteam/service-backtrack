using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Queries.CheckInvitation;

public sealed class CheckInvitationHandler : IRequestHandler<CheckInvitationQuery, CheckInvitationResult>
{
    private readonly IJoinInvitationRepository _invitationRepository;

    public CheckInvitationHandler(IJoinInvitationRepository invitationRepository)
    {
        _invitationRepository = invitationRepository;
    }

    public async Task<CheckInvitationResult> Handle(CheckInvitationQuery query, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByHashCodeAsync(query.Token, cancellationToken);

        if (invitation is null
            || invitation.Status != InvitationStatus.Pending
            || invitation.ExpiredTime < DateTimeOffset.UtcNow
            || !string.Equals(invitation.Email, query.Email, StringComparison.OrdinalIgnoreCase))
        {
            return new CheckInvitationResult { IsTokenValid = false };
        }

        return new CheckInvitationResult
        {
            IsTokenValid = true,
            OrganizationName = invitation.Organization?.Name,
            Role = invitation.Role.ToString(),
        };
    }
}
