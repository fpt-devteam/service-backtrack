using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.DeletePost;

public sealed class DeletePostHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMatchRepository _postMatchRepository;
    private readonly IPostImageRepository _postImageRepository;

    public DeletePostHandler(
        IPostRepository postRepository,
        IPostMatchRepository postMatchRepository,
        IPostImageRepository postImageRepository)
    {
        _postRepository = postRepository;
        _postMatchRepository = postMatchRepository;
        _postImageRepository = postImageRepository;
    }

    public async Task<Unit> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, true);

        if (post == null)
        {
            throw new NotFoundException(PostErrors.NotFound);
        }

        if (post.AuthorId != command.AuthorId)
        {
            throw new ForbiddenException(PostErrors.Forbidden);
        }

        // Delete matches involving this post
        if (post.PostType == PostType.Lost)
        {
            await _postMatchRepository.DeleteByLostPostIdsAsync(new[] { post.Id }, cancellationToken);
        }
        else
        {
            await _postMatchRepository.DeleteByFoundPostIdsAsync(new[] { post.Id }, cancellationToken);
        }

        // Delete all images for this post
        await _postImageRepository.DeleteByPostIdAsync(post.Id, cancellationToken);

        await _postRepository.DeleteAsync(command.PostId);
        await _postRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
