using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.ActiveC2CReturnReport;

public sealed record ActiveC2CReturnReportCommand : IRequest<C2CReturnReportResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid C2CReturnReportId { get; init; }
}
