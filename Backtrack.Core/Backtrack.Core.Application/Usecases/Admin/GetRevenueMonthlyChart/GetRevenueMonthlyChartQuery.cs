using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueMonthlyChart;

public sealed record GetRevenueMonthlyChartQuery : IRequest<List<MonthlyRevenueChartItem>>
{
    public required string AdminUserId { get; init; }
    public int Months { get; init; } = 12;
}
