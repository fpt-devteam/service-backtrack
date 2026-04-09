using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SemanticSearchPost;

public sealed record SemanticSearchPostCommand : IRequest<IEnumerable<SearchPostResult>>
{
    public required string Query { get; init; }
    public PostFilters? Filters { get; init; }
}
