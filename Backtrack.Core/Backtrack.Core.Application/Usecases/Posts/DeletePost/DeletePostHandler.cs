using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.Posts.DeletePost;

public sealed class DeletePostHandler(
    IPostRepository postRepository,
    IPostMatchRepository postMatchRepository,
    IMembershipRepository membershipRepository,
    ILogger<DeletePostHandler> logger) : IRequestHandler<DeletePostCommand>
{
    public async Task<Unit> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(command.PostId, true)
            ?? throw new NotFoundException(PostErrors.NotFound);
        logger.LogInformation("Attempting to delete post {PostId} by user {UserId}", post.Id, command.UserId);
        if (post.OrganizationId.HasValue)
        {
            var membership = await membershipRepository.GetByOrgAndUserAsync(post.OrganizationId.Value, command.UserId, cancellationToken);
            if (membership is null) throw new ForbiddenException(PostErrors.Forbidden);
        }
        else
        {
            logger.LogWarning("Post {PostId} does not belong to any organization", post.Id);
            logger.LogWarning("User {UserId} attempted to delete post {PostId} without organization membership", command.UserId, post.Id);
            if (post.AuthorId != command.UserId) throw new ForbiddenException(PostErrors.Forbidden);
        }

        await postMatchRepository.DeleteByPostIdAsync(post.Id, cancellationToken);

        await postRepository.DeleteAsync(command.PostId);
        await postRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
