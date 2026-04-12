using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetUsers;

public sealed record GetUsersQuery : IRequest<PagedResult<AdminUserSummaryResult>>
{
    public required string AdminUserId { get; init; }
    public string? Search { get; init; }
    public UserStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
