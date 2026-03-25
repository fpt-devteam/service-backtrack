using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
namespace Backtrack.Core.Infrastructure.Repositories;

public class UserRepository : CrudRepositoryBase<User, string>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<User> EnsureExistAsync(User user)
    {
        var existingUser = await GetByIdAsync(user.Id);
        if (existingUser is not null)
            return existingUser;

        try
        {
            await CreateAsync(user);
            await SaveChangesAsync();

            return user;
        }
        catch (DbUpdateException ex) when (IsDuplicatePk(ex))
        {
            _context.Entry(user).State = EntityState.Detached;
            var concurrentUser = await GetByIdAsync(user.Id);
            return concurrentUser ?? throw new InvalidOperationException($"User with Id '{user.Id}' was not found after duplicate key exception.");
        }
    }
    private static bool IsDuplicatePk(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}
