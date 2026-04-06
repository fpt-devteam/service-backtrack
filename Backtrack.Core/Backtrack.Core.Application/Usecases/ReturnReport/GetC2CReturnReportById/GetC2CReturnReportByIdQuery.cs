using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportById;

public sealed record GetC2CReturnReportByIdQuery : IRequest<C2CReturnReportResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public Guid C2CReturnReportId { get; init; }
}
