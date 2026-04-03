using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostsByAuthorId;

public sealed record GetPostsByAuthorIdQuery(string AuthorId) : IRequest<PagedResult<PostResult>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
