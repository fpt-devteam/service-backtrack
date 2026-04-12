using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetGrowthChart;

public enum GrowthChartEntity { Users, Orgs }

public sealed record GetGrowthChartQuery : IRequest<GrowthChartResult>
{
    public required string           AdminUserId { get; init; }
    public required GrowthChartEntity Entity     { get; init; }
    public int Months { get; init; } = 12;
}
