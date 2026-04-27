using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrgDashboardStats;

public sealed class GetOrgDashboardStatsHandler(
    IMembershipRepository      membershipRepository,
    IPostRepository            postRepository,
    IOrgReturnReportRepository orgReturnReportRepository)
    : IRequestHandler<GetOrgDashboardStatsQuery, OrgDashboardStatsResult>
{
    public async Task<OrgDashboardStatsResult> Handle(
        GetOrgDashboardStatsQuery query, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        if (membership.Role != MembershipRole.OrgAdmin)
            throw new ForbiddenException(MembershipErrors.InsufficientRole);

        var now          = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfToday = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

        var totalStaff        = await membershipRepository.CountByOrgAsync(query.OrgId, cancellationToken);
        var activeStaff       = await membershipRepository.CountActiveByOrgAsync(query.OrgId, cancellationToken);
        var newStaffThisMonth = await membershipRepository.CountNewByOrgSinceAsync(query.OrgId, startOfMonth, cancellationToken);

        var totalItems    = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId }, cancellationToken);
        var inStorage     = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, Status = PostStatus.InStorage }, cancellationToken);
        var expiredItems  = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, Status = PostStatus.Expired }, cancellationToken);
        var returnedTotal = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, Status = PostStatus.Returned }, cancellationToken);
        var lostPosts     = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, PostType = PostType.Lost }, cancellationToken);
        var foundPosts    = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, PostType = PostType.Found }, cancellationToken);
        var foundToday    = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, PostType = PostType.Found, Time = new TimeFilter(startOfToday, null) }, cancellationToken);

        var returnedThisMonth = await orgReturnReportRepository.CountByOrgSinceAsync(query.OrgId, startOfMonth, cancellationToken);

        var returnRate = totalItems > 0 ? (int)Math.Round(returnedTotal * 100.0 / totalItems) : 0;

        return new OrgDashboardStatsResult
        {
            TotalStaff        = totalStaff,
            ActiveStaff       = activeStaff,
            NewStaffThisMonth = newStaffThisMonth,
            TotalItems        = totalItems,
            InStorage         = inStorage,
            ReturnedThisMonth = returnedThisMonth,
            ExpiredItems      = expiredItems,
            ReturnRate        = returnRate,
            FoundToday        = foundToday,
            LostPosts         = lostPosts,
            FoundPosts        = foundPosts
        };
    }
}
