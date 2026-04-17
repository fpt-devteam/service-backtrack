using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.DeletePost;

public sealed class DeletePostHandler(
    IPostRepository postRepository,
    IPostMatchRepository postMatchRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<DeletePostCommand>
{
    public async Task<Unit> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(command.PostId, true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId.HasValue)
        {
            var membership = await membershipRepository.GetByOrgAndUserAsync(post.OrganizationId.Value, command.UserId, cancellationToken);
            if (membership is null) throw new ForbiddenException(PostErrors.Forbidden);
        }
        else
        {
            if (post.AuthorId != command.UserId) throw new ForbiddenException(PostErrors.Forbidden);
        }

        // Delete all matches where this post is the source
        await postMatchRepository.DeleteBySourcePostIdsAsync(new[] { post.Id }, cancellationToken);
        // Also delete matches where this post is a candidate
        await postMatchRepository.DeleteByCandidatePostIdsAsync(new[] { post.Id }, cancellationToken);

        await postRepository.DeleteAsync(command.PostId);
        await postRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
