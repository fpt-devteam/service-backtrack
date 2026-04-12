using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetUserDetail;

public sealed record GetUserDetailQuery : IRequest<AdminUserDetailResult>
{
    public required string AdminUserId { get; init; }
    public required string TargetUserId { get; init; }
    public int BillingPage { get; init; } = 1;
    public int BillingPageSize { get; init; } = 20;
}
