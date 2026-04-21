using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.OwnerConfirmC2CReturnReport;

public sealed class OwnerConfirmC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository,
    IPostRepository postRepository,
    IEventPublisher eventPublisher) : IRequestHandler<OwnerConfirmC2CReturnReportCommand, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(OwnerConfirmC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        // Single query — includes FinderPost.Author and OwnerPost.Author
        var returnReport = await returnReportRepository.GetByIdWithPostsAsync(command.C2CReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.Status == C2CReturnReportStatus.Confirmed)
            throw new ValidationException(ReturnReportErrors.AlreadyConfirmed);

        if (returnReport.Status == C2CReturnReportStatus.Expired)
            throw new ValidationException(ReturnReportErrors.AlreadyExpired);

        if (returnReport.Status != C2CReturnReportStatus.Delivered)
            throw new ValidationException(new Error("NotDelivered", "Only a Delivered return report can be confirmed."));

        if (returnReport.OwnerId != command.UserId)
            throw new ForbiddenException(new Error("NotOwner", "Only the owner can confirm the handover."));

        // Confirm the return report
        returnReport.Status = C2CReturnReportStatus.Confirmed;
        returnReport.ConfirmedAt = DateTimeOffset.UtcNow;

        if (returnReport.FinderPost != null)
        {
            returnReport.FinderPost.Status = PostStatus.Returned;
            postRepository.Update(returnReport.FinderPost);
        }

        if (returnReport.OwnerPost != null)
        {
            returnReport.OwnerPost.Status = PostStatus.Returned;
            postRepository.Update(returnReport.OwnerPost);
        }

        await returnReportRepository.SaveChangesAsync();

        var finder = returnReport.FinderPost?.Author ?? returnReport.Finder!;
        var owner = returnReport.OwnerPost?.Author ?? returnReport.Owner!;

        await eventPublisher.PublishReturnReportSyncAsync(new ReturnReportSyncIntegrationEvent
        {
            C2CReturnReportId  = returnReport.Id,
            FinderId           = finder.Id,
            FinderDisplayName  = finder.DisplayName,
            FinderAvatarUrl    = finder.AvatarUrl,
            FinderEmail        = finder.Email,
            OwnerId            = owner.Id,
            OwnerDisplayName   = owner.DisplayName,
            OwnerAvatarUrl     = owner.AvatarUrl,
            OwnerEmail         = owner.Email,
            FinderPostId       = returnReport.FinderPostId,
            FinderPostType     = returnReport.FinderPost?.PostType.ToString(),
            OwnerPostId        = returnReport.OwnerPostId,
            OwnerPostType      = returnReport.OwnerPost?.PostType.ToString(),
            Status             = returnReport.Status.ToString(),
            ActivatedByRole    = null,
            ConfirmedAt        = returnReport.ConfirmedAt,
            ExpiresAt          = returnReport.ExpiresAt,
            CreatedAt          = returnReport.CreatedAt,
            EventTimestamp     = DateTimeOffset.UtcNow,
        });

        // Publish event so that the post service closes both posts and awards points
        var finderId = returnReport.FinderPost?.AuthorId ?? returnReport.FinderId;
        await eventPublisher.PublishReturnReportConfirmedAsync(new ReturnReportConfirmedIntegrationEvent
        {
            C2CReturnReportId = returnReport.Id,
            FinderPostId = returnReport.FinderPostId,
            OwnerPostId = returnReport.OwnerPostId,
            FinderId = finderId,
            EventTimestamp = DateTimeOffset.UtcNow
        });

        return new C2CReturnReportResult
        {
            Id = returnReport.Id,
            Finder = finder.ToUserResult(),
            Owner = owner.ToUserResult(),
            FinderPost = returnReport.FinderPost?.ToPostResult(),
            OwnerPost = returnReport.OwnerPost?.ToPostResult(),
            Status = returnReport.Status.ToString(),
            ActivatedByRole = null,
            ConfirmedAt = returnReport.ConfirmedAt,
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }
}
