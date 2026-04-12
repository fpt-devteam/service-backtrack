using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetPostOverview;

public sealed record GetPostOverviewQuery : IRequest<PostDetailOverviewResult>
{
    public required string AdminUserId { get; init; }
    public int Months { get; init; } = 12;
}
