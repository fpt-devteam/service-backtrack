using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueMonthly;

public sealed record GetRevenueMonthlyQuery : IRequest<List<RevenueMonthlyItemResult>>
{
    public required string AdminUserId { get; init; }
    public int Months { get; init; } = 12;
}
