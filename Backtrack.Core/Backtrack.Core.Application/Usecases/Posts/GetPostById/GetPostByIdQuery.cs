using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostById;

public sealed record GetPostByIdQuery : IRequest<PostResult>
{
    public required Guid PostId { get; init; }
    public string? UserId { get; init; }
}
