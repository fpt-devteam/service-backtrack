using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchInventories;

public sealed class SearchInventoriesHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository) : IRequestHandler<SearchInventoriesCommand, PagedResult<SearchInventoryResult>>
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

        List<Post> posts;
        int totalCount;

        if (!string.IsNullOrWhiteSpace(command.Query))
        {
            var allItems = (await postRepository.SearchByFullTextAsync(
                searchTerm: command.Query,
                filters: filters,
                cancellationToken: cancellationToken)).ToList();

            totalCount = allItems.Count;
            posts = allItems.Skip(pagedQuery.Offset).Take(pagedQuery.Limit).ToList();
        }
        else
        {
            var (items, count) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);
            totalCount = count;
            posts = items.ToList();
        }

        var receiveReports = await receiveReportRepository.GetByPostIdsAsync(
            posts.Select(p => p.Id), cancellationToken);

        var results = posts.Select(p =>
        {
            receiveReports.TryGetValue(p.Id, out var report);
            return MapToResult(p, report?.FinderInfo);
        }).ToList();

        return new PagedResult<SearchInventoryResult>(totalCount, results);
    }

    private static SearchInventoryResult MapToResult(Post post, FinderInfo? finderInfo) => new()
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
        CreatedAt       = post.CreatedAt,
        FinderInfo      = finderInfo
    };
}
