using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Dev.JoinOrganization;

public sealed class DevJoinOrganizationHandler(
    IMembershipRepository membershipRepository,
    IOrganizationRepository organizationRepository)
    : IRequestHandler<DevJoinOrganizationCommand, DevJoinOrganizationResult>
{
    public async Task<DevJoinOrganizationResult> Handle(DevJoinOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(command.OrganizationId);
        if (organization is null)
            throw new NotFoundException(OrganizationErrors.NotFound);

        var existing = await membershipRepository.GetByOrgAndUserAsync(command.OrganizationId, command.UserId, cancellationToken);
        if (existing is not null)
            throw new ConflictException(MembershipErrors.AlreadyAMember);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            OrganizationId = command.OrganizationId,
            UserId = command.UserId,
            Role = command.Role,
            Status = MembershipStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow,
        };

        await membershipRepository.CreateAsync(membership);
        await membershipRepository.SaveChangesAsync();

        return new DevJoinOrganizationResult
        {
            MembershipId = membership.Id,
            OrganizationId = membership.OrganizationId,
            UserId = membership.UserId,
            Role = membership.Role.ToString(),
        };
    }
}
