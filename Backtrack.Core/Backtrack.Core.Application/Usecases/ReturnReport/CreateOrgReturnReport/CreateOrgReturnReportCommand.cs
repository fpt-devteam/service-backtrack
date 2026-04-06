using System.Text.Json.Serialization;
using Backtrack.Core.Application.Usecases.ReturnReport;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CreateOrgReturnReport;

public sealed record CreateOrgReturnReportCommand : IRequest<OrgReturnReportResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; } 

    public required Guid PostId { get; init; }
    public OwnerInfo? OwnerInfo { get; init; }
}
