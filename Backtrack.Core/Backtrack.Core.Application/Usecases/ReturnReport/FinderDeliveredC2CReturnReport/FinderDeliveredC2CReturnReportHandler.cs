using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Notifications.SendPushNotification;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.FinderDeliveredC2CReturnReport;

public sealed class FinderDeliveredC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository,
    IMediator mediator) : IRequestHandler<FinderDeliveredC2CReturnReportCommand, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(FinderDeliveredC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        var returnReport = await returnReportRepository.GetByIdWithPostsAsync(command.C2CReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.Status != C2CReturnReportStatus.Ongoing)
            throw new ValidationException(ReturnReportErrors.NotOngoing);

        if (returnReport.FinderId != command.UserId)
            throw new ForbiddenException(ReturnReportErrors.NotFinder);

        returnReport.Status = C2CReturnReportStatus.Delivered;
        returnReport.DeliveredAt = DateTimeOffset.UtcNow;
        returnReport.EvidenceImageUrls = command.EvidenceImageUrls;

        await returnReportRepository.SaveChangesAsync();

        await mediator.Send(new SendPushNotificationCommand
        {
            Target = new NotificationTarget { UserId = returnReport.Owner.Id },
            Title  = "Item has been delivered!",
            Body   = $"{returnReport.Finder.DisplayName ?? "The finder"} has delivered your item. Please confirm receipt.",
            Type   = NotificationEvent.SystemAlertEvent,
            Data   = new NotificationData { ScreenPath = $"/handovers/{returnReport.Id}" },
            Source = new NotificationSource { Name = "ReturnReport", EventId = $"{returnReport.Id}:delivered" }
        }, cancellationToken);

        return returnReport.ToC2CReturnReportResult();
    }
}
