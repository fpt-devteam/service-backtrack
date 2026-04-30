using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
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

        if (returnReport.Status == C2CReturnReportStatus.Confirmed)
            throw new ValidationException(ReturnReportErrors.AlreadyConfirmed);

        if (returnReport.Status == C2CReturnReportStatus.Rejected)
            throw new ValidationException(ReturnReportErrors.AlreadyRejected);

        if (returnReport.Status == C2CReturnReportStatus.Expired)
            throw new ValidationException(ReturnReportErrors.AlreadyExpired);

        var isParticipant = returnReport.FinderId == command.UserId || returnReport.OwnerId == command.UserId;
        if (!isParticipant)
            throw new ForbiddenException(ReturnReportErrors.NotParticipant);

        returnReport.Status = C2CReturnReportStatus.Rejected;

        await returnReportRepository.SaveChangesAsync();

        return returnReport.ToC2CReturnReportResult();
    }
}
