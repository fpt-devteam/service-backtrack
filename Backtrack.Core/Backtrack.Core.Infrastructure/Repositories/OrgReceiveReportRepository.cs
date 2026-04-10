using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class OrgReceiveReportRepository : CrudRepositoryBase<OrgReceiveReport, Guid>, IOrgReceiveReportRepository
{
    public OrgReceiveReportRepository(ApplicationDbContext context) : base(context) { }

    public async Task<OrgReceiveReport?> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReceiveReport>()
            .Include(r => r.Organization)
            .Include(r => r.Staff)
            .Include(r => r.Post)
            .FirstOrDefaultAsync(r => r.PostId == postId && r.DeletedAt == null, cancellationToken);
    }

    public async Task<Dictionary<Guid, OrgReceiveReport>> GetByPostIdsAsync(IEnumerable<Guid> postIds, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReceiveReport>()
            .Where(r => postIds.Contains(r.PostId) && r.DeletedAt == null)
            .ToDictionaryAsync(r => r.PostId, cancellationToken);
    }
}
