using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrgMonthlyActivity;

public sealed record GetOrgMonthlyActivityQuery : IRequest<List<MonthlyActivityPoint>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }

    public int Months { get; init; } = 12;
}
