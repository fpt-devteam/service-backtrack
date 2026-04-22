using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetPostMonthly;

public sealed class GetPostMonthlyHandler(
    IUserRepository userRepository,
    IPostRepository postRepository) : IRequestHandler<GetPostMonthlyQuery, List<PostMonthlyResult>>
{
    private static readonly string[] MonthAbbr =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public async Task<List<PostMonthlyResult>> Handle(GetPostMonthlyQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        if (query.Months < 1 || query.Months > 24)
            throw new ValidationException(AdminErrors.InvalidMonthsRange);

        var rows = await postRepository.GetPostMonthlyAsync(query.Months, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        
        var periods = Enumerable.Range(0, query.Months)
            .Select(i => startOfThisMonth.AddMonths(-(query.Months - 1 - i)))
            .ToList();

        var lookup = rows
            .GroupBy(r => (r.Year, r.Month))
            .ToDictionary(
                g => g.Key,
                g => (
                    Lost:  g.Where(r => r.PostType == PostType.Lost).Sum(r => r.Count),
                    Found: g.Where(r => r.PostType == PostType.Found).Sum(r => r.Count)
                ));

        return periods.Select(p =>
        {
            var (lost, found) = lookup.TryGetValue((p.Year, p.Month), out var v) ? v : (0, 0);
            return new PostMonthlyResult(
                Month: MonthAbbr[p.Month - 1],
                Year:  p.Year,
                Lost:  lost,
                Found: found
            );
        }).ToList();
    }
}
