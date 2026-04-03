using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostsByAuthorId;

public sealed class GetPostsByAuthorIdHandler(IPostRepository postRepository)
    : IRequestHandler<GetPostsByAuthorIdQuery, PagedResult<PostResult>>
{
    public async Task<PagedResult<PostResult>> Handle(GetPostsByAuthorIdQuery query, CancellationToken cancellationToken)
    {
        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);
        var filters = new PostFilters { AuthorId = query.AuthorId };
        var (items, totalCount) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

        var results = items.Select(post => new PostResult
        {
            Id = post.Id,
            Author = post.Author?.ToPostAuthorResult(),
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType,
            Item = post.Item,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        }).ToList();

        return new PagedResult<PostResult>(totalCount, results);
    }
}
