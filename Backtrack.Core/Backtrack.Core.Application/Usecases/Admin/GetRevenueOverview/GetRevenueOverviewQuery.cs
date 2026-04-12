using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueOverview;

public sealed record GetRevenueOverviewQuery : IRequest<RevenueOverviewResult>
{
    public required string AdminUserId { get; init; }
    public int Months { get; init; } = 12;
}
