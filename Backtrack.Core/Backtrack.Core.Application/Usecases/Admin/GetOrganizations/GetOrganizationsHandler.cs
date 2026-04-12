using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrganizations;

public sealed class GetOrganizationsHandler(
    IUserRepository userRepository,
    IOrganizationRepository organizationRepository,
    IMembershipRepository membershipRepository,
    IPostRepository postRepository) : IRequestHandler<GetOrganizationsQuery, PagedResult<AdminOrgSummaryResult>>
{
    public async Task<PagedResult<AdminOrgSummaryResult>> Handle(GetOrganizationsQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var (orgs, total) = await organizationRepository.GetPagedAdminAsync(
            query.Page, query.PageSize, query.Search, query.Status, cancellationToken);

        var orgIds = orgs.Select(o => o.Id).ToList();

        var memberCounts = await Task.WhenAll(orgIds.Select(id =>
            membershipRepository.CountByOrgAsync(id, cancellationToken)));

        var postCounts = await Task.WhenAll(orgIds.Select(id =>
            postRepository.CountAsync(new PostFilters { OrganizationId = id }, cancellationToken)));

        var results = orgs.Select((org, i) => new AdminOrgSummaryResult
        {
            Id          = org.Id,
            Name        = org.Name,
            Slug        = org.Slug,
            LogoUrl     = org.LogoUrl,
            Status      = org.Status,
            MemberCount = memberCounts[i],
            PostCount   = postCounts[i],
            CreatedAt   = org.CreatedAt
        }).ToList();

        return new PagedResult<AdminOrgSummaryResult>(total, results);
    }
}
