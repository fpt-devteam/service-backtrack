using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;

public sealed record InitiateC2CReturnReportCommand : IRequest<C2CReturnReportResult>
{
    [JsonIgnore]
    public string InitiatorId { get; init; } = string.Empty;
    public required Guid FinderPostId { get; init; }
    public required Guid OwnerPostId { get; init; }
}
