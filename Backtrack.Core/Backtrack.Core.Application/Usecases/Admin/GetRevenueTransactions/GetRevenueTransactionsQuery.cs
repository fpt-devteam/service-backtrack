using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueTransactions;

public sealed record GetRevenueTransactionsQuery : IRequest<RevenueTransactionsPageResult>
{
    public required string AdminUserId { get; init; }
    public int Page     { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
