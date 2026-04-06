using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetOrgReturnReportById;

public sealed record GetOrgReturnReportByIdQuery : IRequest<OrgReturnReportResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }

    public Guid ReturnReportId { get; init; }
}
