using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class PaymentHistoryRepository : CrudRepositoryBase<PaymentHistory, Guid>, IPaymentHistoryRepository
{
    public PaymentHistoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<PaymentHistory>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

    public async Task<(decimal Total, decimal ThisMonth, decimal LastMonth, decimal UserRevenue, decimal OrgRevenue)>
        GetRevenueSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now          = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfLast  = startOfMonth.AddMonths(-1);

        var rows = await _dbSet.AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Succeeded)
            .Select(p => new
            {
                p.Amount,
                p.PaymentDate,
                p.SubscriberType
            })
            .ToListAsync(cancellationToken);

        var total      = rows.Sum(p => p.Amount);
        var thisMonth  = rows.Where(p => p.PaymentDate >= startOfMonth).Sum(p => p.Amount);
        var lastMonth  = rows.Where(p => p.PaymentDate >= startOfLast && p.PaymentDate < startOfMonth).Sum(p => p.Amount);
        var userRev    = rows.Where(p => p.SubscriberType == SubscriberType.User).Sum(p => p.Amount);
        var orgRev     = rows.Where(p => p.SubscriberType == SubscriberType.Organization).Sum(p => p.Amount);

        return (total, thisMonth, lastMonth, userRev, orgRev);
    }

    public async Task<Dictionary<Guid, decimal>> GetRevenueSumsByOrgIdsAsync(
        IEnumerable<Guid> orgIds,
        CancellationToken cancellationToken = default)
    {
        var ids  = orgIds.ToList();
        var rows = await _dbSet.AsNoTracking()
            .Where(p => p.OrganizationId.HasValue && ids.Contains(p.OrganizationId!.Value))
            .GroupBy(p => p.OrganizationId!.Value)
            .Select(g => new { OrgId = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(r => r.OrgId, r => r.Total);
    }

    public async Task<List<(string Period, decimal Amount, int TransactionCount)>> GetRevenueChartAsync(
        int months, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-months);
        const string sql = @"
            SELECT TO_CHAR(DATE_TRUNC('month', payment_date), 'YYYY-MM') AS period,
                   SUM(amount) AS amount,
                   COUNT(*)::int AS transaction_count
            FROM payment_histories
            WHERE status = 'Succeeded'
              AND payment_date >= @cutoff
            GROUP BY DATE_TRUNC('month', payment_date)
            ORDER BY 1";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var result = new List<(string, decimal, int)>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new Npgsql.NpgsqlParameter("@cutoff", cutoff));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetString(0), reader.GetDecimal(1), reader.GetInt32(2)));

        return result;
    }

    public async Task<List<(int Year, int Month, decimal Org, decimal User)>> GetRevenueMonthlyAsync(
        int months, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-months);
        const string sql = @"
            SELECT EXTRACT(YEAR  FROM payment_date)::int AS year,
                   EXTRACT(MONTH FROM payment_date)::int AS month,
                   SUM(CASE WHEN subscriber_type = 'Organization' THEN amount ELSE 0 END) AS org_revenue,
                   SUM(CASE WHEN subscriber_type = 'User'         THEN amount ELSE 0 END) AS user_revenue
            FROM payment_histories
            WHERE status = 'Succeeded'
              AND payment_date >= @cutoff
            GROUP BY year, month
            ORDER BY year, month";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var result = new List<(int, int, decimal, decimal)>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new Npgsql.NpgsqlParameter("@cutoff", cutoff));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2), reader.GetDecimal(3)));

        return result;
    }

    public async Task<(List<PaymentHistory> Items, int Total)> GetPagedByUserIdAsync(
        string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(p => p.Subscription)
            .Where(p => p.UserId == userId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(List<PaymentHistory> Items, int Total)> GetPagedByOrgIdAsync(
        Guid orgId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(p => p.Subscription)
            .Where(p => p.OrganizationId == orgId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
