using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.JoinByInvitation;

public sealed class JoinByInvitationHandler : IRequestHandler<JoinByInvitationCommand, JoinByInvitationResult>
{
    private readonly IJoinInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;

    public JoinByInvitationHandler(
        IJoinInvitationRepository invitationRepository,
        IMembershipRepository membershipRepository)
    {
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<JoinByInvitationResult> Handle(JoinByInvitationCommand command, CancellationToken cancellationToken)
    {
        // Look up invitation by hash code (with tracking for update)
        var invitation = await _invitationRepository.GetByHashCodeAsync(command.Token, cancellationToken);
        if (invitation is null)
        {
            throw new NotFoundException(InvitationErrors.InvitationNotFound);
        }

        // Validate status
        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ValidationException(InvitationErrors.InvitationNotPending);
        }

        // Validate not expired
        if (invitation.ExpiredTime < DateTimeOffset.UtcNow)
        {
            throw new ValidationException(InvitationErrors.InvitationExpired);
        }

        // Validate email matches
        if (!string.Equals(invitation.Email, command.UserEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(InvitationErrors.EmailMismatch);
        }

        // Check user not already a member
        var existingMembership = await _membershipRepository.GetByOrgAndUserAsync(
            invitation.OrganizationId, command.UserId, cancellationToken);
        if (existingMembership is not null)
        {
            throw new ConflictException(MembershipErrors.AlreadyAMember);
        }

        // Create membership
        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            OrganizationId = invitation.OrganizationId,
            UserId = command.UserId,
            Role = invitation.Role,
            Status = MembershipStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow,
        };

        await _membershipRepository.CreateAsync(membership);

        // Update invitation status to Accepted
        invitation.Status = InvitationStatus.Accepted;
        invitation.UpdatedAt = DateTimeOffset.UtcNow;

        await _invitationRepository.SaveChangesAsync();

        return new JoinByInvitationResult
        {
            OrganizationId = invitation.OrganizationId,
            MembershipId = membership.Id,
            Role = membership.Role.ToString(),
        };
    }
}
