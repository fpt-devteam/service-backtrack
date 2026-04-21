using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.PublishInventoryItem;

public sealed class PublishInventoryItemHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository,
    IOrgReturnReportRepository returnReportRepository,
    IBackgroundJobService backgroundJobService) : IRequestHandler<PublishInventoryItemCommand, InventoryItemResult>
{
    public async Task<InventoryItemResult> Handle(PublishInventoryItemCommand command, CancellationToken cancellationToken)
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

        var receiveReport = await receiveReportRepository.GetByPostIdAsync(post.Id, cancellationToken);
        var returnReport  = await returnReportRepository.GetByPostIdAsync(post.Id, cancellationToken);

        return post.ToInventoryItemResult(receiveReport, returnReport);
    }
}
