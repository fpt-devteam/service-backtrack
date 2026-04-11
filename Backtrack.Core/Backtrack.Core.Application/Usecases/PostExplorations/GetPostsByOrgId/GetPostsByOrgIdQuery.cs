using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.GetPostsByOrgId;

public sealed record GetPostsByOrgIdQuery : IRequest<PagedResult<PostResult>>
{
    public required Guid OrgId { get; init; }
    public PostType? PostType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
