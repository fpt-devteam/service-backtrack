using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CloseC2CReturnReport;

public sealed class CloseC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<CloseC2CReturnReportCommand, Unit>
{
    public async Task<Unit> Handle(CloseC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        var report = await returnReportRepository.GetByIdAsync(command.ReturnReportId);
        if (report is null) return Unit.Value;

        if (report.Status is C2CReturnReportStatus.Confirmed
            or C2CReturnReportStatus.Rejected
            or C2CReturnReportStatus.Expired
            or C2CReturnReportStatus.Closed)
            return Unit.Value;

        report.Status = C2CReturnReportStatus.Closed;
        await returnReportRepository.SaveChangesAsync();
        return Unit.Value;
    }
}
