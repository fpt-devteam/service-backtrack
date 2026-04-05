using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class OrgReturnReportRepository : CrudRepositoryBase<OrgReturnReport, Guid>, IOrgReturnReportRepository
{
    public OrgReturnReportRepository(ApplicationDbContext context) : base(context) { }

    public async Task<OrgReturnReport?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .Include(r => r.Organization)
            .Include(r => r.Staff)
            .Include(r => r.Post)
                .ThenInclude(p => p!.Author)
            .FirstOrDefaultAsync(r => r.Id == id && r.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> ExistsActiveForPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .AnyAsync(r => r.PostId == postId && r.DeletedAt == null, cancellationToken);
    }
}
