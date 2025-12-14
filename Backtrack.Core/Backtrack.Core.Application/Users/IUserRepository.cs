using Backtrack.Core.Application.Common.Interfaces;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Users
{
    public interface IUserRepository : IGenericRepository<User, string>
    {
    }
}
