using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRecentActivity;

public sealed record GetRecentActivityQuery : IRequest<RecentActivityResult>
{
    public string AdminUserId { get; init; } = string.Empty;
    public string? Status { get; init; }
    public int Limit { get; init; } = 10;
}
