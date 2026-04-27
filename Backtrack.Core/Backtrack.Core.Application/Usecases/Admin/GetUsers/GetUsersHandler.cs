using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetUsers;

public sealed class GetUsersHandler(
    IUserRepository userRepository) : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var (users, realTotal) = await userRepository.GetPagedAsync(query.Page, query.PageSize, query.Search, query.Status, cancellationToken);
        var anonymousCount     = await userRepository.CountAnonymousAsync(cancellationToken);
        var totalPages         = (int)Math.Ceiling(realTotal / (double)query.PageSize);

        var items = users.Select(u => new AdminUserSummaryResult
        {
            Id          = u.Id,
            Email       = u.Email,
            DisplayName = u.DisplayName,
            AvatarUrl   = u.AvatarUrl,
            Status      = u.Status,
            GlobalRole  = u.GlobalRole,
            CreatedAt   = u.CreatedAt
        }).ToList();

        return new GetUsersResult
        {
            Items          = items,
            TotalCount     = realTotal,
            Page           = query.Page,
            PageSize       = query.PageSize,
            TotalPages     = totalPages,
            RealUserCount  = realTotal,
            AnonymousCount = anonymousCount,
        };
    }
}
