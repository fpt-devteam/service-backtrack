using Backtrack.Core.Application.Usecases.PostExplorations;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.FullTextSearchPost;

public sealed record FullTextSearchPostCommand : IRequest<IEnumerable<SearchPostResult>>
{
    public required string Query { get; init; }
    public PostFilters? Filters { get; init; }
}
