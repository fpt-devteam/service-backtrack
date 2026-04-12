using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetUsers;

public sealed class GetUsersHandler(
    IUserRepository userRepository) : IRequestHandler<GetUsersQuery, PagedResult<AdminUserSummaryResult>>
{
    public async Task<PagedResult<AdminUserSummaryResult>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var (users, total) = await userRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.Status,
            cancellationToken);

        var results = users.Select(u => new AdminUserSummaryResult
        {
            Id          = u.Id,
            Email       = u.Email,
            DisplayName = u.DisplayName,
            AvatarUrl   = u.AvatarUrl,
            Status      = u.Status,
            GlobalRole  = u.GlobalRole,
            CreatedAt   = u.CreatedAt
        }).ToList();

        return new PagedResult<AdminUserSummaryResult>(total, results);
    }
}
