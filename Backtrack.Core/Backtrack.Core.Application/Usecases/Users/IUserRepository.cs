using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.Users;

public interface IUserRepository : IGenericRepository<User, string>
{
    Task<User> EnsureExistAsync(User user);
}
