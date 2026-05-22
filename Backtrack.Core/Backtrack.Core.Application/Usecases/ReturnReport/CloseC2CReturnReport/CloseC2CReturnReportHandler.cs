using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CloseC2CReturnReport;

public sealed class CloseC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<CloseC2CReturnReportCommand, Unit>
{
    public async Task<Unit> Handle(CloseC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        var report = await returnReportRepository.GetByIdAsync(command.ReturnReportId, isTrack: true)
            ?? throw new ValidationException(ReturnReportErrors.NotFound);
        if (report is null) throw new ValidationException(ReturnReportErrors.NotFound);
        if (command.UserId != report.FinderId && command.UserId != report.OwnerId) throw new ValidationException(ReturnReportErrors.NotOwnPostInReport);

        if (report.Status is not C2CReturnReportStatus.Ongoing) throw new ValidationException(ReturnReportErrors.CloseForOngoingOnly);

        report.Status = C2CReturnReportStatus.Closed;
        report.ClosedAt = DateTimeOffset.UtcNow;
        await returnReportRepository.SaveChangesAsync();
        return Unit.Value;
    }
}
