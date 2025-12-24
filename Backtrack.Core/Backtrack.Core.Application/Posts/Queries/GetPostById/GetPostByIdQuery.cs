using Backtrack.Core.Application.Posts.Common;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.GetPostById;

public sealed record GetPostByIdQuery : IRequest<PostResult>
{
    public required Guid PostId { get; init; }
}
