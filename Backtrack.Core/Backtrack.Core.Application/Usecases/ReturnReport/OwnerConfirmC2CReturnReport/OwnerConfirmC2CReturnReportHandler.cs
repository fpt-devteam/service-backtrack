using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Notifications.SendPushNotification;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.OwnerConfirmC2CReturnReport;

public sealed class OwnerConfirmC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository,
    IPostRepository postRepository,
    IMediator mediator) : IRequestHandler<OwnerConfirmC2CReturnReportCommand, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(OwnerConfirmC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        var returnReport = await returnReportRepository.GetByIdWithPostsAsync(command.C2CReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.Status == C2CReturnReportStatus.Confirmed)
            throw new ValidationException(ReturnReportErrors.AlreadyConfirmed);

        if (returnReport.Status == C2CReturnReportStatus.Expired)
            throw new ValidationException(ReturnReportErrors.AlreadyExpired);

        if (returnReport.Status != C2CReturnReportStatus.Delivered)
            throw new ValidationException(ReturnReportErrors.NotDelivered);

        if (returnReport.OwnerId != command.UserId)
            throw new ForbiddenException(ReturnReportErrors.NotOwner);

        returnReport.Status = C2CReturnReportStatus.Confirmed;
        returnReport.ConfirmedAt = DateTimeOffset.UtcNow;

        returnReport.FinderPost.Status = PostStatus.Returned;
        postRepository.Update(returnReport.FinderPost);

        returnReport.OwnerPost.Status = PostStatus.Returned;
        postRepository.Update(returnReport.OwnerPost);

        await returnReportRepository.SaveChangesAsync();

        var handoverData = new NotificationData { ScreenPath = $"/handovers/{returnReport.Id}" };
        var handoverSource = new NotificationSource { Name = "ReturnReport", EventId = string.Empty };

        await mediator.Send(new SendPushNotificationCommand
        {
            Target = new NotificationTarget { UserId = returnReport.Finder.Id },
            Title  = "Handover confirmed!",
            Body   = $"{returnReport.Owner.DisplayName ?? "The owner"} has confirmed receipt of your item. Thank you!",
            Type   = NotificationEvent.SystemAlertEvent,
            Data   = handoverData,
            Source = handoverSource with { EventId = $"{returnReport.Id}:confirmed:finder" }
        }, cancellationToken);

        await mediator.Send(new SendPushNotificationCommand
        {
            Target = new NotificationTarget { UserId = returnReport.Owner.Id },
            Title  = "Handover complete!",
            Body   = "You have successfully confirmed receipt of your item.",
            Type   = NotificationEvent.SystemAlertEvent,
            Data   = handoverData,
            Source = handoverSource with { EventId = $"{returnReport.Id}:confirmed:owner" }
        }, cancellationToken);

        return returnReport.ToC2CReturnReportResult();
    }
}
