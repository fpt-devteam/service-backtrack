using Backtrack.Core.Application.Users;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Backtrack.Core.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
namespace Backtrack.Core.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger) : CrudRepositoryBase<User, string>(context), IUserRepository
{
    public async Task<User> UpsertAsync(User user)
    {
        try
        {
            await CreateAsync(user);
            await SaveChangesAsync();

            return user;
        }
        catch (DbUpdateException ex) when (IsDuplicatePk(ex))
        {
            context.Entry(user).State = EntityState.Detached;

            var existingUser = await context.Set<User>()
                .SingleAsync(x => x.Id == user.Id);

            existingUser.Email = user.Email ?? existingUser.Email;
            existingUser.DisplayName = user.DisplayName ?? existingUser.DisplayName;
            existingUser.Status = user.Status;
            existingUser.GlobalRole = user.GlobalRole;

            await SaveChangesAsync();

            return existingUser;
        }
    }
    private static bool IsDuplicatePk(DbUpdateException ex)
    => ex.InnerException is PostgresException pg
    && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}
