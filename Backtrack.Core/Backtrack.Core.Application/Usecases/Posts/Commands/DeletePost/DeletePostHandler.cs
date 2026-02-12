using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Commands.DeletePost;

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
