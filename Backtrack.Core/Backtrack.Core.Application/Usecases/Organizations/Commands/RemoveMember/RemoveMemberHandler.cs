using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.RemoveMember;

public sealed class RemoveMemberHandler : IRequestHandler<RemoveMemberCommand, Unit>
{
    private readonly IMembershipRepository _membershipRepository;

    public RemoveMemberHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<Unit> Handle(RemoveMemberCommand command, CancellationToken cancellationToken)
    {
        var callerMembership = await _membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (callerMembership is null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }
        if (callerMembership.Role != MembershipRole.OrgAdmin)
        {
            throw new ForbiddenException(MembershipErrors.InsufficientRole);
        }

        var targetMembership = await _membershipRepository.GetByIdAsync(command.TargetMembershipId);
        if (targetMembership is null || targetMembership.OrganizationId != command.OrgId)
        {
            throw new NotFoundException(MembershipErrors.MemberNotFound);
        }

        if (targetMembership.Role == MembershipRole.OrgAdmin && targetMembership.Status == MembershipStatus.Active)
        {
            var adminCount = await _membershipRepository.CountActiveAdminsAsync(command.OrgId, cancellationToken);
            if (adminCount <= 1)
            {
                throw new ValidationException(MembershipErrors.CannotRemoveLastAdmin);
            }
        }

        await _membershipRepository.DeleteAsync(command.TargetMembershipId);
        await _membershipRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
