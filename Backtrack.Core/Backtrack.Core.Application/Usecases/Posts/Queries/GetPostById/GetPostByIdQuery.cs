using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Queries.GetPostById;

public sealed record GetPostByIdQuery : IRequest<PostResult>
{
    public required Guid PostId { get; init; }
}
