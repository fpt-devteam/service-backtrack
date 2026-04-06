using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetOrgReturnReports;

public sealed record GetOrgReturnReportsQuery : IRequest<PagedResult<OrgReturnReportResult>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
