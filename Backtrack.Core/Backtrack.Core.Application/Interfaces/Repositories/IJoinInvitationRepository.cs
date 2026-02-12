using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IJoinInvitationRepository : IGenericRepository<JoinInvitation, Guid>
{
    Task<JoinInvitation?> GetByHashCodeAsync(string hashCode, CancellationToken cancellationToken = default);
    Task<JoinInvitation?> GetPendingByEmailAndOrgAsync(string email, Guid orgId, CancellationToken cancellationToken = default);
}
