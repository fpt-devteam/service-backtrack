using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetStaffDashboardStats;

public sealed class GetStaffDashboardStatsHandler(
    IMembershipRepository       membershipRepository,
    IPostRepository             postRepository,
    IOrgReturnReportRepository  orgReturnReportRepository)
    : IRequestHandler<GetStaffDashboardStatsQuery, StaffDashboardStatsResult>
{
    public async Task<StaffDashboardStatsResult> Handle(
        GetStaffDashboardStatsQuery query, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var startOfWeek = DateTimeOffset.UtcNow.AddDays(-7);

        var myItemsInStorage = await postRepository.CountAsync(new PostFilters
        {
            AuthorId       = query.UserId,
            OrganizationId = query.OrgId,
            Status         = PostStatus.InStorage
        }, cancellationToken);

        var myItemsTotal = await postRepository.CountAsync(new PostFilters
        {
            AuthorId       = query.UserId,
            OrganizationId = query.OrgId
        }, cancellationToken);

        var pendingReturns = await orgReturnReportRepository.CountPendingByStaffAsync(
            query.OrgId, query.UserId, cancellationToken);

        var returnedThisWeek = await orgReturnReportRepository.CountByOrgSinceAsync(
            query.OrgId, startOfWeek, cancellationToken);

        return new StaffDashboardStatsResult
        {
            MyItemsInStorage = myItemsInStorage,
            MyItemsTotal     = myItemsTotal,
            PendingReturns   = pendingReturns,
            ReturnedThisWeek = returnedThisWeek
        };
    }
}
