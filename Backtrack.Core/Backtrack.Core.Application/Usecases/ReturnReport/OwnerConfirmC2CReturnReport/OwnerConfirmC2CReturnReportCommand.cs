using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.OwnerConfirmC2CReturnReport;

public sealed record OwnerConfirmC2CReturnReportCommand : IRequest<C2CReturnReportResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid C2CReturnReportId { get; init; }

}
