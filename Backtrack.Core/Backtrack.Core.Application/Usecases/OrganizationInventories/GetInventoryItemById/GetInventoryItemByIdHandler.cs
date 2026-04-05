using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetInventoryItemById;

public sealed class GetInventoryItemByIdHandler(
    IOrganizationInventoryRepository inventoryRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<GetInventoryItemByIdQuery, OrganizationInventoryResult>
{
    public async Task<OrganizationInventoryResult> Handle(GetInventoryItemByIdQuery query, CancellationToken cancellationToken)
    {
        // Authorization check
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        var inv = await inventoryRepository.GetByIdAsync(query.Id)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (inv.OrgId != query.OrgId)
        {
            throw new ForbiddenException(PostErrors.Forbidden);
        }

        return new OrganizationInventoryResult
        {
            Id = inv.Id,
            OrgId = inv.OrgId,
            LoggedById = inv.LoggedById,
            ItemName = inv.ItemName,
            Description = inv.Description,
            DistinctiveMarks = inv.DistinctiveMarks,
            ImageUrls = inv.ImageUrls,
            StorageLocation = inv.StorageLocation,
            Status = inv.Status.ToString(),
            LoggedAt = inv.LoggedAt,
            CreatedAt = inv.CreatedAt,
            ReceiverStaffId = inv.ReceiverStaffId,
            HandoverStaffId = inv.HandoverStaffId,
            FinderContact = inv.FinderContact == null ? null : new FinderContactResult
            {
                Id = inv.FinderContact.Id,
                Name = inv.FinderContact.Name,
                Email = inv.FinderContact.Email,
                Phone = inv.FinderContact.Phone,
                NationalId = inv.FinderContact.NationalId,
                OrgMemberId = inv.FinderContact.OrgMemberId,
            }
        };
    }
}
