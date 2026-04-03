using Backtrack.Core.Application.Usecases.PostExplorations;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchPosts;

public sealed record SearchPostsCommand : IRequest<IEnumerable<SearchPostResult>>
{
    public required string Query { get; init; }
    public PostFilters? Filters { get; init; }
}
