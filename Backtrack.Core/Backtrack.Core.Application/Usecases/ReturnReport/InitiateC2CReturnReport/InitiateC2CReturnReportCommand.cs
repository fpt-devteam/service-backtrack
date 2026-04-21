using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;

public sealed record InitiateC2CReturnReportCommand : IRequest<C2CReturnReportResult>
{
    [JsonIgnore]
    public string InitiatorId { get; init; } = string.Empty;
    public Guid? FinderPostId { get; init; }
    public Guid? OwnerPostId { get; init; }
    public string? FinderId { get; init; }
    public string? OwnerId { get; init; }
}
