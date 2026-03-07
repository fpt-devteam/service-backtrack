using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.DeletePost;

public sealed record DeletePostCommand : IRequest
{
    public required Guid PostId { get; init; }
    public string AuthorId { get; init; } = string.Empty;
}
