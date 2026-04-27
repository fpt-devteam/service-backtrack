using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrgMonthlyActivity;

public sealed class GetOrgMonthlyActivityHandler(
    IMembershipRepository membershipRepository,
    IPostRepository       postRepository)
    : IRequestHandler<GetOrgMonthlyActivityQuery, List<MonthlyActivityPoint>>
{
    private static readonly string[] MonthAbbr =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public async Task<List<MonthlyActivityPoint>> Handle(
        GetOrgMonthlyActivityQuery query, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        if (membership.Role != MembershipRole.OrgAdmin)
            throw new ForbiddenException(MembershipErrors.InsufficientRole);

        var rows = await postRepository.GetMonthlyActivityByOrgAsync(query.OrgId, query.Months, cancellationToken);

        var now              = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var periods = Enumerable.Range(0, query.Months)
            .Select(i => startOfThisMonth.AddMonths(-(query.Months - 1 - i)))
            .ToList();

        var lookup = rows.ToDictionary(r => (r.Year, r.Month), r => (r.Lost, r.Found, r.Returned));

        return periods.Select(p =>
        {
            var (lost, found, returned) = lookup.TryGetValue((p.Year, p.Month), out var v) ? v : (0, 0, 0);
            return new MonthlyActivityPoint(MonthAbbr[p.Month - 1], lost, found, returned);
        }).ToList();
    }
}
