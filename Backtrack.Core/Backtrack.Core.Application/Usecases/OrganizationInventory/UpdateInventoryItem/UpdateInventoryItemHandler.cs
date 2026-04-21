using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.UpdateInventoryItem;

public sealed class UpdateInventoryItemHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository,
    IOrgReturnReportRepository returnReportRepository,
    IBackgroundJobService backgroundJobService,
    IHasher hasher) : IRequestHandler<UpdateInventoryItemCommand, InventoryItemResult>
{
    public async Task<InventoryItemResult> Handle(UpdateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership is null) throw new ForbiddenException(MembershipErrors.NotAMember);

        var post = await postRepository.GetByIdAsync(command.PostId, true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId != command.OrgId)
            throw new ForbiddenException(PostErrors.Forbidden);

        bool needsReEmbedding = false;

        if (command.PersonalBelongingDetail != null)
        {
            UpdateDetail(post, command.PersonalBelongingDetail);
            post.PostTitle = post.PersonalBelongingDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        else if (command.CardDetail != null)
        {
            UpdateDetail(post, command.CardDetail, hasher);
            post.PostTitle = post.CardDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        else if (command.ElectronicDetail != null)
        {
            UpdateDetail(post, command.ElectronicDetail);
            post.PostTitle = post.ElectronicDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        if (command.OtherDetail != null)
        {
            UpdateDetail(post, command.OtherDetail);
            post.PostTitle = post.OtherDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }

        if (command.PostTitle != null && post.PostTitle != command.PostTitle)
        {
            post.PostTitle = command.PostTitle;
            needsReEmbedding = true;
        }

        if (command.ImageUrls != null)
        {
            post.ImageUrls = command.ImageUrls.ToList();
            needsReEmbedding = true;
        }

        if (command.Status is not null && Enum.TryParse<PostStatus>(command.Status, ignoreCase: true, out var parsedStatus))
            post.Status = parsedStatus;

        post.EventTime = command.EventTime ?? post.EventTime;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        if (needsReEmbedding)
        {
            post.EmbeddingStatus    = EmbeddingStatus.Pending;
            post.PostMatchingStatus = PostMatchingStatus.Pending;
        }

        await postRepository.SaveChangesAsync();

        if (needsReEmbedding) backgroundJobService.EnqueueJob(new UpdatePostEmbeddingCommand(post.Id));

        var receiveReport = await receiveReportRepository.GetByPostIdAsync(post.Id, cancellationToken);
        var returnReport  = await returnReportRepository.GetByPostIdAsync(post.Id, cancellationToken);

        return post.ToInventoryItemResult(receiveReport, returnReport);
    }

    private static void UpdateDetail(Post post, PersonalBelongingDetailDto input)
    {
        if (post.PersonalBelongingDetail is { } d) input.ApplyTo(d);
        else post.PersonalBelongingDetail = input.ToEntity(post.Id);
    }

    private static void UpdateDetail(Post post, CardDetailDto input, IHasher hasher)
    {
        if (post.CardDetail is { } d) input.ApplyTo(d, hasher);
        else post.CardDetail = input.ToEntity(post.Id, hasher);
    }

    private static void UpdateDetail(Post post, ElectronicDetailDto input)
    {
        if (post.ElectronicDetail is { } d) input.ApplyTo(d);
        else post.ElectronicDetail = input.ToEntity(post.Id);
    }

    private static void UpdateDetail(Post post, OtherDetailDto input)
    {
        if (post.OtherDetail is { } d) input.ApplyTo(d);
        else post.OtherDetail = input.ToEntity(post.Id);
    }
}
