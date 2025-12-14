using Backtrack.Core.Application.Users;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Backtrack.Core.Infrastructure.Repositories.Common;
namespace Backtrack.Core.Infrastructure.Repositories
{
    public class UserRepository(ApplicationDbContext context) : CrudRepositoryBase<User, string>(context), IUserRepository
    {
    }
}