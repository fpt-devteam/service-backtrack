using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Commands.DeletePost;

public sealed record DeletePostCommand : IRequest
{
    public required Guid PostId { get; init; }
}
