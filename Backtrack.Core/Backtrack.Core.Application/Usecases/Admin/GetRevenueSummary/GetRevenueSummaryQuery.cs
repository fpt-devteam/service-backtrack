using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueSummary;

public sealed record GetRevenueSummaryQuery : IRequest<RevenueSummaryResult>
{
    public required string AdminUserId { get; init; }
}
