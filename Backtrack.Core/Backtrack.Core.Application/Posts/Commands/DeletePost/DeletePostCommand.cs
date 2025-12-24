using MediatR;

namespace Backtrack.Core.Application.Posts.Commands.DeletePost;

public sealed record DeletePostCommand : IRequest
{
    public required Guid PostId { get; init; }
}
