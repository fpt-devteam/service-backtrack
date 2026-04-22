using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrganizations;

public sealed class GetOrganizationsHandler(
    IUserRepository userRepository,
    IOrganizationRepository organizationRepository,
    ISubscriptionRepository subscriptionRepository,
    IMembershipRepository membershipRepository,
    IPostRepository postRepository,
    IPaymentHistoryRepository paymentHistoryRepository) : IRequestHandler<GetOrganizationsQuery, OrganizationsResult>
{
    public async Task<OrganizationsResult> Handle(GetOrganizationsQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        if (query.Page < 1)
            throw new ValidationException(AdminErrors.InvalidPage);

        if (query.PageSize > 50)
            throw new ValidationException(AdminErrors.InvalidPageSize);

        // Load all orgs matching search filter (pagination applied in memory after status filtering)
        var allOrgs   = await organizationRepository.GetAllForAdminAsync(query.Search, cancellationToken);
        var allOrgIds = allOrgs.Select(o => o.Id).ToList();

        // Bulk load subscriptions to compute business status per org
        var allSubs = await subscriptionRepository.GetByOrgIdsAsync(allOrgIds, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var latestSubByOrg = allSubs
            .GroupBy(s => s.OrganizationId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.CurrentPeriodEnd).First());

        // Filter by Organization.Status (the entity field — Active / Suspended)
        var filtered = query.Status switch
        {
            OrganizationStatus.Active    => allOrgs.Where(o => o.Status == OrganizationStatus.Active).ToList(),
            OrganizationStatus.Suspended => allOrgs.Where(o => o.Status == OrganizationStatus.Suspended).ToList(),
            _                            => allOrgs
        };

        var totalCount = filtered.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var pageOrgs   = filtered.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();
        var pageOrgIds = pageOrgs.Select(o => o.Id).ToList();

        if (pageOrgs.Count == 0)
            return new OrganizationsResult
            {
                Items      = [],
                TotalCount = totalCount,
                Page       = query.Page,
                PageSize   = query.PageSize,
                TotalPages = totalPages
            };

        // Bulk load detail data for the current page only
        var memberships  = await membershipRepository.GetByOrgIdsWithUserAsync(pageOrgIds, cancellationToken);
        var memberCounts = await membershipRepository.GetCountsByOrgIdsAsync(pageOrgIds, cancellationToken);
        var postStats    = await postRepository.GetStatsByOrgIdsAsync(pageOrgIds, cancellationToken);
        var revenueSums  = await paymentHistoryRepository.GetRevenueSumsByOrgIdsAsync(pageOrgIds, cancellationToken);

        var adminEmailByOrg = memberships
            .Where(m => m.Role == MembershipRole.OrgAdmin)
            .GroupBy(m => m.OrganizationId)
            .ToDictionary(g => g.Key, g => g.First().User?.Email);

        var items = pageOrgs.Select(org =>
        {
            var sub      = latestSubByOrg.TryGetValue(org.Id, out var s) ? s : null;
            var isActive = sub is not null && sub.CurrentPeriodEnd > now;
            var (total, ret) = postStats.TryGetValue(org.Id, out var ps) ? ps : (0, 0);
            var successRate  = total > 0 ? Math.Round(ret / (double)total * 100, 1) : 0.0;
            var revenue      = revenueSums.TryGetValue(org.Id, out var rev) ? (long)rev : 0L;
            var memberCount  = memberCounts.TryGetValue(org.Id, out var mc) ? mc : 0;
            var adminEmail   = adminEmailByOrg.TryGetValue(org.Id, out var email) ? email : null;

            return new OrganizationItemResult
            {
                Id               = org.Id,
                Name             = org.Name,
                LogoUrl          = org.LogoUrl,
                AdminEmail       = adminEmail,
                SubscriptionPlan = sub?.PlanSnapshot.Name,
                Status           = org.Status.ToString(),
                Capacity         = new OrganizationCapacityResult(memberCount, 0),
                Performance      = successRate,
                TotalRevenue     = revenue,
                SuccessRate      = successRate,
                NextBilling      = isActive ? sub!.CurrentPeriodEnd : null,
                CreatedAt        = org.CreatedAt
            };
        }).ToList();

        return new OrganizationsResult
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = query.Page,
            PageSize   = query.PageSize,
            TotalPages = totalPages
        };
    }
}
