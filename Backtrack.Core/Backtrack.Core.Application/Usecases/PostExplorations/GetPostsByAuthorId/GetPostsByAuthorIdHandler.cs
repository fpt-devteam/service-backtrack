using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.GetPostsByAuthorId;

public sealed class GetPostsByAuthorIdHandler(IPostRepository postRepository)
    : IRequestHandler<GetPostsByAuthorIdQuery, PagedResult<PostResult>>
{
    public async Task<PagedResult<PostResult>> Handle(GetPostsByAuthorIdQuery query, CancellationToken cancellationToken)
    {
        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);
        var filters = new PostFilters { AuthorId = query.AuthorId };
        var (items, totalCount) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

        var results = items.Select(post => post.ToPostResult()).ToList();

        return new PagedResult<PostResult>(totalCount, results);
    }
}
