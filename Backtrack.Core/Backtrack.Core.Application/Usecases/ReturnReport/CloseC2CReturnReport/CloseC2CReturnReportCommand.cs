using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CloseC2CReturnReport;

public sealed record CloseC2CReturnReportCommand : IRequest<Unit>
{
    public required Guid ReturnReportId { get; init; }
}
