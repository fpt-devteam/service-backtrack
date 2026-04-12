using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.RejectC2CReturnReport;

public sealed class RejectC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<RejectC2CReturnReportCommand, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(RejectC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        var returnReport = await returnReportRepository.GetByIdWithPostsAsync(command.C2CReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.Status == ReturnReportStatus.Confirmed)
            throw new ValidationException(ReturnReportErrors.AlreadyConfirmed);

        if (returnReport.Status == ReturnReportStatus.Rejected)
            throw new ValidationException(ReturnReportErrors.AlreadyRejected);

        if (returnReport.Status == ReturnReportStatus.Expired)
            throw new ValidationException(ReturnReportErrors.AlreadyExpired);

        var isParticipant = returnReport.FinderId == command.UserId || returnReport.OwnerId == command.UserId;
        if (!isParticipant)
            throw new ForbiddenException(ReturnReportErrors.NotParticipant);

        returnReport.Status = ReturnReportStatus.Rejected;

        await returnReportRepository.SaveChangesAsync();

        var finder = returnReport.FinderPost?.Author ?? returnReport.Finder!;
        var owner = returnReport.OwnerPost?.Author ?? returnReport.Owner!;

        return new C2CReturnReportResult
        {
            Id = returnReport.Id,
            Finder = finder.ToUserResult(),
            Owner = owner.ToUserResult(),
            FinderPost = returnReport.FinderPost?.ToPostResult(),
            OwnerPost = returnReport.OwnerPost?.ToPostResult(),
            Status = returnReport.Status.ToString(),
            ActivatedByRole = returnReport.ActivatedById == returnReport.FinderId ? "Finder"
                            : returnReport.ActivatedById == returnReport.OwnerId ? "Owner"
                            : null,
            ConfirmedAt = returnReport.ConfirmedAt,
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }
}
