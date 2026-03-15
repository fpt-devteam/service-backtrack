using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.SearchInventoryBySemantic;

public sealed class SearchInventoryBySemanticHandler(
    IOrganizationInventoryRepository inventoryRepository,
    IMembershipRepository membershipRepository,
    IEmbeddingService embeddingService) : IRequestHandler<SearchInventoryBySemanticQuery, PagedResult<InventorySemanticSearchResult>>
{
    public async Task<PagedResult<InventorySemanticSearchResult>> Handle(SearchInventoryBySemanticQuery query, CancellationToken cancellationToken)
    {
        // Authorization check
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);

        // Generate embedding for search text with enhanced context
        var enhancedQuery = $@"I am searching for: {query.SearchText}
Item description: {query.SearchText}

Looking for information about {query.SearchText.ToLower()}.";

        var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(enhancedQuery, cancellationToken);

        var (items, totalCount) = await inventoryRepository.SearchBySemanticAsync(
            queryEmbedding: queryEmbedding,
            offset: pagedQuery.Offset,
            limit: pagedQuery.Limit,
            orgId: query.OrgId,
            cancellationToken: cancellationToken);

        var results = items.Select(item =>
        {
            var (inv, similarityScore) = item;
            return new InventorySemanticSearchResult
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
                SimilarityScore = similarityScore
            };
        }).ToList();

        return new PagedResult<InventorySemanticSearchResult>(totalCount, results);
    }
}
