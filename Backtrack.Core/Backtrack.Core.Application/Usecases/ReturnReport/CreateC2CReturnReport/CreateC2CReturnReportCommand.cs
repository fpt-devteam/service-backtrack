using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CreateC2CReturnReport;

public sealed record CreateC2CReturnReportCommand : IRequest<C2CReturnReportResult>
{
    [JsonIgnore]
    public string InitiatorId { get; init; } = string.Empty;
    public Guid? FinderPostId { get; init; }
    public Guid? OwnerPostId { get; init; }
    public string? FinderId { get; init; }
    public string? OwnerId { get; init; }
    public required ReturnReportStatus Status { get; init; }
}
