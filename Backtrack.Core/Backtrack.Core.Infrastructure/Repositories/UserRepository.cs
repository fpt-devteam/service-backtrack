using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
namespace Backtrack.Core.Infrastructure.Repositories;

public class UserRepository : CrudRepositoryBase<User, string>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<User> EnsureExistAsync(User user)
    {
        var existingUser = await GetByIdAsync(user.Id, isTrack: true);
        if (existingUser is not null)
            return existingUser;

        try
        {
            await CreateAsync(user);
            await SaveChangesAsync();

            return user;
        }
        catch (DbUpdateException ex) when (IsDuplicatePk(ex))
        {
            _context.Entry(user).State = EntityState.Detached;
            var concurrentUser = await GetByIdAsync(user.Id);
            return concurrentUser ?? throw new InvalidOperationException($"User with Id '{user.Id}' was not found after duplicate key exception.");
        }
    }
    private static bool IsDuplicatePk(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    public async Task<(int Total, int Active, int NewThisMonth)> GetCountsAsync(CancellationToken cancellationToken = default)
    {
        var startOfMonth = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var total        = await _dbSet.CountAsync(cancellationToken);
        var active       = await _dbSet.CountAsync(u => u.Status == UserStatus.Active, cancellationToken);
        var newThisMonth = await _dbSet.CountAsync(u => u.CreatedAt >= startOfMonth, cancellationToken);

        return (total, active, newThisMonth);
    }

    public async Task<List<(string Period, int Count)>> GetGrowthChartAsync(int months, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-months);
        var sql = @"
            SELECT TO_CHAR(DATE_TRUNC('month', created_at), 'YYYY-MM') AS period,
                   COUNT(*)::int AS count
            FROM users
            WHERE created_at >= @cutoff
            GROUP BY DATE_TRUNC('month', created_at)
            ORDER BY 1";

        var conn = _context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var result = new List<(string, int)>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new Npgsql.NpgsqlParameter("@cutoff", cutoff));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add((reader.GetString(0), reader.GetInt32(1)));

        return result;
    }

    public async Task<int> CountAnonymousAsync(CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(u => u.Email == null || u.Email == string.Empty, cancellationToken);

    public async Task<(List<User> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        // Only real (non-anonymous) users — anonymous users have no email
        var query = _dbSet.AsNoTracking().Where(u => u.Email != null && u.Email != string.Empty);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(u =>
                (u.DisplayName != null && u.DisplayName.ToLower().Contains(term)) ||
                (u.Email       != null && u.Email.ToLower().Contains(term)));
        }

        if (status != null)
            query = query.Where(u => u.Status == status);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
