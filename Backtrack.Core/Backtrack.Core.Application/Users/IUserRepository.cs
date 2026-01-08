using Backtrack.Core.Application.Common.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Users
{
    public interface IUserRepository : IGenericRepository<User, string>
    {
        Task<User> UpsertAsync(User user);
    }
}
