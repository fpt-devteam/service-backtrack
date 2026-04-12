using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.PublishInventoryPost;

public sealed class PublishInventoryPostHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IBackgroundJobService backgroundJobService) : IRequestHandler<PublishInventoryPostCommand, PostResult>
{
    public async Task<PostResult> Handle(PublishInventoryPostCommand command, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership is null) throw new ForbiddenException(MembershipErrors.NotAMember);

        var post = await postRepository.GetByIdAsync(command.PostId, isTrack: true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId != command.OrgId)
            throw new ForbiddenException(PostErrors.Forbidden);

        if (post.Status != PostStatus.InStorage)
            throw new ConflictException(PostErrors.NotInStorage);

        post.Status    = PostStatus.Active;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        postRepository.Update(post);
        await postRepository.SaveChangesAsync();

        backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(
            orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return new PostResult
        {
            Id              = post.Id,
            Author          = post.Author?.ToPostAuthorResult(),
            Organization    = post.Organization?.ToOrganizationOnPost(),
            PostType        = post.PostType,
            Status          = post.Status,
            Item            = post.Item,
            ImageUrls       = post.ImageUrls,
            Location        = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress  = post.DisplayAddress,
            EventTime       = post.EventTime,
            CreatedAt       = post.CreatedAt
        };
    }
}
