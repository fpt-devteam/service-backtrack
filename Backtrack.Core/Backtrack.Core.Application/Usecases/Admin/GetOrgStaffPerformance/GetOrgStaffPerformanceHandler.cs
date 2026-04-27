using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrgStaffPerformance;

public sealed class GetOrgStaffPerformanceHandler(
    IMembershipRepository membershipRepository,
    IPostRepository       postRepository)
    : IRequestHandler<GetOrgStaffPerformanceQuery, List<StaffPerformanceItemResult>>
{
    public async Task<List<StaffPerformanceItemResult>> Handle(
        GetOrgStaffPerformanceQuery query, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        if (membership.Role != MembershipRole.OrgAdmin)
            throw new ForbiddenException(MembershipErrors.InsufficientRole);

        var (members, _) = await membershipRepository.GetPagedByOrgAsync(query.OrgId, 0, int.MaxValue, cancellationToken);
        var staffList    = members.Where(m => m.Status == MembershipStatus.Active).ToList();

        var authorIds = staffList.Select(m => m.UserId).ToList();
        var postStats = await postRepository.GetStatsByAuthorIdsAsync(query.OrgId, authorIds, cancellationToken);

        var since       = DateTimeOffset.UtcNow.AddDays(-90);
        var today       = DateOnly.FromDateTime(DateTime.UtcNow);

        var results = new List<StaffPerformanceItemResult>(staffList.Count);
        foreach (var m in staffList)
        {
            postStats.TryGetValue(m.UserId, out var stats);
            var (total, returned) = stats;

            var returnRate   = total > 0 ? (int)Math.Round(returned * 100.0 / total) : 0;
            var activeDates  = await postRepository.GetActiveDatesAsync(m.UserId, query.OrgId, since, cancellationToken);
            var streak       = ComputeStreak(activeDates, today);

            results.Add(new StaffPerformanceItemResult
            {
                Id          = m.UserId,
                Name        = m.User?.DisplayName ?? string.Empty,
                Role        = m.Role.ToString(),
                ItemsLogged = total,
                ReturnRate  = returnRate,
                ActiveChats = 0,
                Streak      = streak,
            });
        }

        return results.OrderByDescending(r => r.ItemsLogged).ToList();
    }

    private static int ComputeStreak(HashSet<DateOnly> activeDates, DateOnly today)
    {
        var day    = activeDates.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;
        while (activeDates.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }
        return streak;
    }
}
