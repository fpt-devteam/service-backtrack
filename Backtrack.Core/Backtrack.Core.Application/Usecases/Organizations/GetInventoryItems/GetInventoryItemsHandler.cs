using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetInventoryItems;

public sealed class GetInventoryItemsHandler(
    IOrganizationInventoryRepository inventoryRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<GetInventoryItemsQuery, PagedResult<OrganizationInventoryResult>>
{
    public async Task<PagedResult<OrganizationInventoryResult>> Handle(GetInventoryItemsQuery query, CancellationToken cancellationToken)
    {
        // Authorization check
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        OrganizationInventoryStatus? status = null;
        if (query.Status != null && Enum.TryParse<OrganizationInventoryStatus>(query.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var paged = PagedQuery.FromPage(query.Page, query.PageSize);

        var (items, totalCount) = await inventoryRepository.GetPagedAsync(
            paged.Offset,
            paged.Limit,
            orgId: query.OrgId,
            status: status,
            searchTerm: query.SearchTerm,
            cancellationToken: cancellationToken);

        var results = items.Select(inv => new OrganizationInventoryResult
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
            CreatedAt = inv.CreatedAt
        }).ToList();

        return new PagedResult<OrganizationInventoryResult>(totalCount, results);
    }
}
