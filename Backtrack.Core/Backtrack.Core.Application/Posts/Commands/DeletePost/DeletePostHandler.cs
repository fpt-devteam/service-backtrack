using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using MediatR;

namespace Backtrack.Core.Application.Posts.Commands.DeletePost;

public sealed class DeletePostHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;

    public DeletePostHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Unit> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        var deleted = await _postRepository.DeleteAsync(command.PostId);

        if (!deleted)
        {
            throw new NotFoundException(PostErrors.NotFound);
        }

        await _postRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
