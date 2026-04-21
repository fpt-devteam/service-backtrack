using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;

public sealed class SearchInventoryItemsHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository,
    IOrgReturnReportRepository returnReportRepository,
    IEmbeddingService embeddingService) : IRequestHandler<SearchInventoryItemsCommand, PagedResult<InventoryItemResult>>
{
    public async Task<PagedResult<InventoryItemResult>> Handle(SearchInventoryItemsCommand command, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership is null) throw new ForbiddenException(MembershipErrors.NotAMember);

        PagedQuery pagedQuery = PagedQuery.FromPage(command.page, command.pageSize);
        var filters = new PostFilters
        {
            OrganizationId = command.OrgId,
            PostType       = PostType.Found,
            Category       = command.Filters?.Category,
            Status         = command.Filters?.Status,
            AuthorId       = command.Filters?.StaffId,
            Time           = command.Filters?.Time
        };

        List<Post> posts;
        int totalCount;

        if (!string.IsNullOrWhiteSpace(command.Query))
        {
            var embedding = await embeddingService.GenerateQueryEmbeddingAsync(command.Query, cancellationToken);
            var allItems  = (await postRepository.SearchBySemanticAsync(
                queryEmbedding: embedding,
                filters: filters,
                cancellationToken: cancellationToken)).ToList();

            totalCount = allItems.Count;
            posts = allItems.OrderByDescending(x => x.SimilarityScore).Select(x => x.Post).ToList();
        }
        else
        {
            var (items, count) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);
            totalCount = count;
            posts = items.ToList();
        }

        var postIds        = posts.Select(p => p.Id).ToList();
        var receiveReports = await receiveReportRepository.GetByPostIdsAsync(postIds, cancellationToken);
        var returnReports  = await returnReportRepository.GetByPostIdsAsync(postIds, cancellationToken);

        var results = posts.Select(p =>
        {
            receiveReports.TryGetValue(p.Id, out var receiveReport);
            returnReports.TryGetValue(p.Id, out var returnReport);
            return p.ToInventoryItemResult(receiveReport, returnReport);
        }).ToList();

        return new PagedResult<InventoryItemResult>(totalCount, results);
    }
}
