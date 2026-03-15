using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.DeleteInventoryItem;

public sealed class DeleteInventoryItemHandler(
    IOrganizationInventoryRepository inventoryRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<DeleteInventoryItemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteInventoryItemCommand command, CancellationToken cancellationToken)
    {
        // Authorization check
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        var inventory = await inventoryRepository.GetByIdAsync(command.Id, true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (inventory.OrgId != command.OrgId)
        {
            throw new ForbiddenException(PostErrors.Forbidden);
        }

        await inventoryRepository.DeleteAsync(command.Id);
        await inventoryRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
