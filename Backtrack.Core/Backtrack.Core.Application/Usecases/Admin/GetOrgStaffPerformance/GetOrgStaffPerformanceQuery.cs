using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrgStaffPerformance;

public sealed record GetOrgStaffPerformanceQuery : IRequest<List<StaffPerformanceItemResult>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }
}
