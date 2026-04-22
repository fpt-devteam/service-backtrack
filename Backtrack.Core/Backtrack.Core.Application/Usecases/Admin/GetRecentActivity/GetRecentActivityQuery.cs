using Backtrack.Core.Application.Usecases;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRecentActivity;

public sealed record GetRecentActivityQuery : IRequest<PagedResult<RecentActivityItemResult>>
{
    public string AdminUserId { get; init; } = string.Empty;
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
