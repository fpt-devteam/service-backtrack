using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetPendingInvitations;

public sealed class GetPendingInvitationsHandler : IRequestHandler<GetPendingInvitationsQuery, GetPendingInvitationsResult>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IJoinInvitationRepository _invitationRepository;

    public GetPendingInvitationsHandler(
        IMembershipRepository membershipRepository,
        IJoinInvitationRepository invitationRepository)
    {
        _membershipRepository = membershipRepository;
        _invitationRepository = invitationRepository;
    }

    public async Task<GetPendingInvitationsResult> Handle(GetPendingInvitationsQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(query.UserId))
        {
            throw new InvalidOperationException("UserId is not provided when initializing GetPendingInvitationsQuery.");
        }

        // Verify caller is a member of the organization
        var callerMembership = await _membershipRepository.GetByOrgAndUserAsync(query.OrganizationId, query.UserId, cancellationToken);
        if (callerMembership is null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        // Verify caller is OrgAdmin
        if (callerMembership.Role != MembershipRole.OrgAdmin)
        {
            throw new ForbiddenException(MembershipErrors.InsufficientRole);
        }

        // Fetch pending invitations
        var invitations = await _invitationRepository.GetPendingByOrgAsync(query.OrganizationId, cancellationToken);

        return new GetPendingInvitationsResult
        {
            Invitations = invitations.Select(i => new InvitationResponse
            {
                Id = i.Id,
                Email = i.Email,
                Role = i.Role.ToString(),
                Status = i.Status.ToString(),
                ExpiredTime = i.ExpiredTime,
                CreatedAt = i.CreatedAt
            })
        };
    }
}
