using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Backtrack.Core.Infrastructure.Repositories;

public class OrganizationRepository : CrudRepositoryBase<Organization, Guid>, IOrganizationRepository
{
    public OrganizationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(o => o.Slug == slug, cancellationToken);
    }

    public async Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(o => o.Slug == slug, cancellationToken);
    }

    public async Task<(List<Organization> Items, int Total)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(int Total, int Active, int NewThisMonth)> GetCountsAsync(CancellationToken cancellationToken = default)
    {
        var startOfMonth = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var total        = await _dbSet.CountAsync(cancellationToken);
        var active       = await _dbSet.CountAsync(o => o.Status == OrganizationStatus.Active, cancellationToken);
        var newThisMonth = await _dbSet.CountAsync(o => o.CreatedAt >= startOfMonth, cancellationToken);

        return (total, active, newThisMonth);
    }

    public async Task<List<(string Period, int Count)>> GetGrowthChartAsync(int months, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-months);
        const string sql = @"
            SELECT TO_CHAR(DATE_TRUNC('month', created_at), 'YYYY-MM') AS period,
                   COUNT(*)::int AS count
            FROM organizations
            WHERE created_at >= @cutoff
              AND deleted_at IS NULL
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

    public async Task<(List<Organization> Items, int Total)> GetPagedAdminAsync(
        int page,
        int pageSize,
        string? search = null,
        OrganizationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(o =>
                o.Name.ToLower().Contains(term) ||
                o.Slug.ToLower().Contains(term));
        }

        if (status != null)
            query = query.Where(o => o.Status == status);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<List<Organization>> GetAllForAdminAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(o =>
                o.Name.ToLower().Contains(term) ||
                o.Memberships.Any(m =>
                    m.Role == MembershipRole.OrgAdmin &&
                    m.User.Email != null &&
                    m.User.Email.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<OrgContractField>?> GetRequiredFinderContractFieldsByOrgIdAsync(
        Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.Id == orgId)
            .Select(o => o.RequiredFinderContractFields)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<OrgContractField>?> GetRequiredOwnerContractFieldsByOrgIdAsync(
        Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.Id == orgId)
            .Select(o => o.RequiredOwnerContractFields)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
