using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchInventories;

public sealed class SearchInventoriesHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<SearchInventoriesCommand, PagedResult<SearchInventoryResult>>
{
    public async Task<PagedResult<SearchInventoryResult>> Handle(SearchInventoriesCommand command, CancellationToken cancellationToken)
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
        };

        List<SearchInventoryResult> results;
        int totalCount;

        if (!string.IsNullOrWhiteSpace(command.Query))
        {
            var allItems = (await postRepository.SearchByFullTextAsync(
                searchTerm: command.Query,
                filters: filters,
                cancellationToken: cancellationToken)).ToList();

            totalCount = allItems.Count;
            results = allItems.Skip(pagedQuery.Offset).Take(pagedQuery.Limit).Select(MapToResult).ToList();
        }
        else
        {
            var (items, count) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

            totalCount = count;
            results = items.Select(MapToResult).ToList();
        }

        return new PagedResult<SearchInventoryResult>(totalCount, results);
    }

    private static SearchInventoryResult MapToResult(Domain.Entities.Post post) => new()
    {
        Id              = post.Id,
        Author          = post.Author?.ToPostAuthorResult(),
        Organization    = post.Organization?.ToOrganizationOnPost(),
        PostType        = post.PostType,
        Status          = post.Status,
        Item            = post.Item,
        ImageUrls       = post.ImageUrls,
        Location        = post.Location!,
        ExternalPlaceId = post.ExternalPlaceId,
        DisplayAddress  = post.DisplayAddress,
        EventTime       = post.EventTime,
        CreatedAt       = post.CreatedAt
    };
}
