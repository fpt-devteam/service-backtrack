using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.UpdateMemberRole;

public sealed class UpdateMemberRoleHandler : IRequestHandler<UpdateMemberRoleCommand, MemberResult>
{
    private readonly IMembershipRepository _membershipRepository;

    public UpdateMemberRoleHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<MemberResult> Handle(UpdateMemberRoleCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MembershipRole>(command.Role, ignoreCase: true, out var newRole))
        {
            throw new ValidationException(MembershipErrors.InvalidMembershipRole);
        }

        var callerMembership = await _membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (callerMembership is null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }
        if (callerMembership.Role != MembershipRole.OrgAdmin)
        {
            throw new ForbiddenException(MembershipErrors.InsufficientRole);
        }

        var targetMembership = await _membershipRepository.GetByIdAsync(command.TargetMembershipId, isTrack: true);
        if (targetMembership is null || targetMembership.OrganizationId != command.OrgId)
        {
            throw new NotFoundException(MembershipErrors.MemberNotFound);
        }

        if (targetMembership.Role == MembershipRole.OrgAdmin
            && newRole != MembershipRole.OrgAdmin
            && targetMembership.Status == MembershipStatus.Active)
        {
            var adminCount = await _membershipRepository.CountActiveAdminsAsync(command.OrgId, cancellationToken);
            if (adminCount <= 1)
            {
                throw new ValidationException(MembershipErrors.CannotDemoteLastAdmin);
            }
        }

        targetMembership.Role = newRole;
        _membershipRepository.Update(targetMembership);
        await _membershipRepository.SaveChangesAsync();

        return new MemberResult
        {
            MembershipId = targetMembership.Id,
            UserId = targetMembership.UserId,
            DisplayName = targetMembership.User?.DisplayName,
            Email = targetMembership.User?.Email,
            AvatarUrl = targetMembership.User?.AvatarUrl,
            Role = targetMembership.Role.ToString(),
            Status = targetMembership.Status.ToString(),
            JoinedAt = targetMembership.JoinedAt,
        };
    }
}
