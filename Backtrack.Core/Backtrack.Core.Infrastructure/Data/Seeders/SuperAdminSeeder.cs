using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Configurations;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        SuperAdminSettings? settings,
        CancellationToken ct = default)
    {
        if (settings is null)
        {
            logger.LogInformation("SuperAdminSettings not configured — super-admin seeding skipped.");
            return;
        }

        string firebaseUid;
        try
        {
            var existing = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(settings.Email, ct);
            firebaseUid = existing.Uid;
            logger.LogInformation("Super-admin Firebase user already exists (uid={Uid}).", firebaseUid);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            var created = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
            {
                Email         = settings.Email,
                Password      = settings.Password,
                DisplayName   = settings.DisplayName,
                EmailVerified = true,
                Disabled      = false
            }, ct);

            firebaseUid = created.Uid;
            logger.LogInformation("Created super-admin Firebase user (uid={Uid}).", firebaseUid);
        }

        var dbUser = await db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == firebaseUid, ct);

        if (dbUser is null)
        {
            db.Set<User>().Add(new User
            {
                Id          = firebaseUid,
                Email       = settings.Email,
                DisplayName = settings.DisplayName,
                GlobalRole  = UserGlobalRole.PlatformSuperAdmin,
                CreatedAt   = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Super-admin DB record created (uid={Uid}).", firebaseUid);
        }
        else if (dbUser.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
        {
            dbUser.GlobalRole = UserGlobalRole.PlatformSuperAdmin;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Updated super-admin DB role (uid={Uid}).", firebaseUid);
        }
        else
        {
            logger.LogInformation("Super-admin DB record already exists — skipping.");
        }
    }
}
