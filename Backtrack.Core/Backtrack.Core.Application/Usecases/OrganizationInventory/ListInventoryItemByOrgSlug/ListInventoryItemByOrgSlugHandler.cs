using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.ListInventoryItemByOrgSlug;

public sealed class ListInventoryItemByOrgSlugHandler(
    IPostRepository postRepository,
    IOrganizationRepository organizationRepository)
    : IRequestHandler<ListInventoryItemByOrgSlugQuery, PagedResult<PostResult>>
{
    public async Task<PagedResult<PostResult>> Handle(ListInventoryItemByOrgSlugQuery query, CancellationToken cancellationToken)
    {
        var org = await organizationRepository.GetBySlugAsync(query.Slug, cancellationToken)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);
        var filters = new PostFilters
        {
            OrganizationId = org.Id,
            PostType       = Domain.Constants.PostType.Found,
            Status         = Domain.Constants.PostStatus.InStorage
        };

        var (items, totalCount) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

        var results = items.Select(p => p.ToPostResult()).ToList();

        return new PagedResult<PostResult>(totalCount, results);
    }
}
