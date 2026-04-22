using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchPostByTitle;

public sealed record SearchPostByTitleQuery : IRequest<IEnumerable<SearchPostResult>>
{
    public required string Query { get; init; }
    public PostFilters? Filters { get; init; }
}
