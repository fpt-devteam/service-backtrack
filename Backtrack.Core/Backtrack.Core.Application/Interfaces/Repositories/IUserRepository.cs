using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User, string>
{
    Task<User> EnsureExistAsync(User user);
}
