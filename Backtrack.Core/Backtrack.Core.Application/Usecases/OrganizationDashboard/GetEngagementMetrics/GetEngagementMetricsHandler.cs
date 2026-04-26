using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetEngagementMetrics;

public sealed class GetEngagementMetricsHandler(
    IMembershipRepository membershipRepository,
    IPostRepository       postRepository)
    : IRequestHandler<GetEngagementMetricsQuery, EngagementMetricsResult>
{
    public async Task<EngagementMetricsResult> Handle(
        GetEngagementMetricsQuery query, CancellationToken cancellationToken)
    {
        _ = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var today        = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfMonth = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var since60Days  = DateTimeOffset.UtcNow.AddDays(-60);

        var itemsThisMonth = await postRepository.CountAsync(new PostFilters
        {
            AuthorId       = query.UserId,
            OrganizationId = query.OrgId,
            Time           = new TimeFilter(startOfMonth, null)
        }, cancellationToken);

        var activeDates = await postRepository.GetActiveDatesAsync(
            query.UserId, query.OrgId, since60Days, cancellationToken);

        // weekActivity: index 0 = 6 days ago, index 6 = today
        var weekActivity = Enumerable.Range(0, 7)
            .Select(i => activeDates.Contains(today.AddDays(-(6 - i))))
            .ToList();

        // streak: consecutive active days going back from today
        var streak = 0;
        for (var d = today; activeDates.Contains(d); d = d.AddDays(-1))
            streak++;

        var daysElapsed    = (today.DayNumber - DateOnly.FromDateTime(startOfMonth.DateTime).DayNumber) + 1;
        var avgItemsPerDay = Math.Round(itemsThisMonth / (double)daysElapsed, 1);

        var activeDaysThisWeek = weekActivity.Count(x => x);
        var streakScore        = Math.Min(streak / 30.0, 1.0) * 40;
        var consistencyScore   = activeDaysThisWeek / 7.0 * 35;
        var volumeScore        = Math.Min(itemsThisMonth / 30.0, 1.0) * 25;
        var score              = (int)Math.Round(streakScore + consistencyScore + volumeScore);

        var rank = score switch
        {
            >= 85 => "Champion",
            >= 70 => "Active",
            >= 50 => "Regular",
            >= 25 => "Casual",
            _     => "Inactive"
        };

        return new EngagementMetricsResult
        {
            Score          = score,
            Rank           = rank,
            ItemsThisMonth = itemsThisMonth,
            AvgItemsPerDay = avgItemsPerDay,
            WeekActivity   = weekActivity,
            Streak         = streak
        };
    }
}
