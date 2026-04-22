using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.ListPostsByAuthorId;

public sealed record ListPostsByAuthorIdQuery(string AuthorId) : IRequest<PagedResult<PostResult>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
