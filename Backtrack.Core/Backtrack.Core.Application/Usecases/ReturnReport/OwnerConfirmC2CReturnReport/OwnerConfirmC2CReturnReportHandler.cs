using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
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

        if (returnReport.Status == ReturnReportStatus.Confirmed)
        {
            throw new ValidationException(ReturnReportErrors.AlreadyConfirmed);
        }

        if (returnReport.Status == ReturnReportStatus.Expired)
        {
            throw new ValidationException(ReturnReportErrors.AlreadyExpired);
        }

        // Only the owner of the lost post can confirm the return
        if (returnReport.OwnerPost?.AuthorId != command.UserId)
        {
            throw new ForbiddenException(new Error("NotOwner", "Only the owner can confirm this return report."));
        }

        // Confirm the return report
        returnReport.Status = ReturnReportStatus.Confirmed;
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
            Finder = returnReport.FinderPost!.Author.ToUserResult(),
            Owner = returnReport.OwnerPost!.Author.ToUserResult(),
            FinderPost = returnReport.FinderPost.ToPostResult(),
            OwnerPost = returnReport.OwnerPost.ToPostResult(),
            Status = returnReport.Status.ToString(),
            ConfirmedAt = returnReport.ConfirmedAt,
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }
}
