using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class JoinInvitationRepository : CrudRepositoryBase<JoinInvitation, Guid>, IJoinInvitationRepository
{
    public JoinInvitationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<JoinInvitation?> GetByHashCodeAsync(string hashCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(j => j.Organization)
            .FirstOrDefaultAsync(j => j.HashCode == hashCode, cancellationToken);
    }

    public async Task<JoinInvitation?> GetPendingByEmailAndOrgAsync(string email, Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(j =>
                j.Email == email &&
                j.OrganizationId == orgId &&
                j.Status == InvitationStatus.Pending &&
                j.ExpiredTime > DateTimeOffset.UtcNow,
                cancellationToken);
    }
}
