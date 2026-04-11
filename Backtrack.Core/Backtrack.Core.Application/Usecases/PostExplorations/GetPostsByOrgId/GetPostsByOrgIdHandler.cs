using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.GetPostsByOrgId;

public sealed class GetPostsByOrgIdHandler(
    IPostRepository postRepository,
    IOrganizationRepository organizationRepository)
    : IRequestHandler<GetPostsByOrgIdQuery, PagedResult<PostResult>>
{
    public async Task<PagedResult<PostResult>> Handle(GetPostsByOrgIdQuery query, CancellationToken cancellationToken)
    {
        var orgExists = await organizationRepository.GetByIdAsync(query.OrgId) is not null;
        if (!orgExists) throw new NotFoundException(OrganizationErrors.NotFound);

        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);
        var filters = new PostFilters
        {
            OrganizationId = query.OrgId,
            PostType = query.PostType,
            Status = Domain.Constants.PostStatus.Active
        };

        var (items, totalCount) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

        var results = items.Select(post => new PostResult
        {
            Id = post.Id,
            Author = post.Author?.ToPostAuthorResult(),
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType,
            Status = post.Status,
            Item = post.Item,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt,
        }).ToList();

        return new PagedResult<PostResult>(totalCount, results);
    }
}
