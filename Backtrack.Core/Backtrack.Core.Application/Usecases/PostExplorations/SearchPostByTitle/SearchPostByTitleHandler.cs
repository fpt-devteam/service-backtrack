using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchPostByTitle;

public sealed class SearchPostByTitleHandler(IPostRepository postRepository)
    : IRequestHandler<SearchPostByTitleQuery, IEnumerable<SearchPostResult>>
{
    public async Task<IEnumerable<SearchPostResult>> Handle(
        SearchPostByTitleQuery query,
        CancellationToken cancellationToken)
    {
        var posts = await postRepository.SearchByTitleAsync(query.Query, query.Filters, cancellationToken);

        return posts.Select(p => p.ToSearchPostResult());
    }
}
