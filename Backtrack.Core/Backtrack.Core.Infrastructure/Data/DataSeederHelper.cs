using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Application.Usecases.Users.EnsureUserExist;
using Backtrack.Core.Domain.Entities;
using FirebaseAdmin.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data;

public sealed record SeededUserResult
{
    public required UserResult User { get; init; }
    public required string CustomToken { get; init; }
    public required bool WasCreated { get; init; }
}

public static class DataSeederHelper
{
    public static async Task<SeededUserResult> SeedUserAsync(
        ApplicationDbContext db,
        string email,
        string password,
        string displayName,
        ISender mediator,
        ILogger logger,
        string? avatarUrl = null,
        CancellationToken ct = default)
    {
        var existingDbUser = await db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (existingDbUser is not null)
        {
            logger.LogInformation("User {Email} already in DB — skipping Firebase, syncing only.", email);

            var userResult = await mediator.Send(new EnsureUserExistCommand
            {
                UserId      = existingDbUser.Id,
                Email       = existingDbUser.Email,
                DisplayName = existingDbUser.DisplayName,
                AvatarUrl   = avatarUrl ?? existingDbUser.AvatarUrl
            }, ct);

            return new SeededUserResult { User = userResult, CustomToken = string.Empty, WasCreated = false };
        }

        var firebaseUser = await EnsureFirebaseUserAsync(email, password, displayName, logger, ct);

        if (!firebaseUser.EmailVerified)
        {
            await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs
            {
                Uid           = firebaseUser.Uid,
                EmailVerified = true
            }, ct);
        }

        var customToken = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(firebaseUser.Uid, ct);

        var result = await mediator.Send(new EnsureUserExistCommand
        {
            UserId      = firebaseUser.Uid,
            Email       = firebaseUser.Email,
            DisplayName = firebaseUser.DisplayName,
            AvatarUrl   = avatarUrl ?? firebaseUser.PhotoUrl
        }, ct);

        logger.LogInformation("User {Email} created and synced to DB (uid={Uid}).", email, firebaseUser.Uid);

        return new SeededUserResult { User = result, CustomToken = customToken, WasCreated = true };
    }

    private static async Task<UserRecord> EnsureFirebaseUserAsync(
        string email, string password, string displayName, ILogger logger, CancellationToken ct)
    {
        try
        {
            var existing = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, ct);
            logger.LogInformation("Firebase user already exists (uid={Uid}).", existing.Uid);
            return existing;
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            var created = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
            {
                Email         = email,
                Password      = password,
                DisplayName   = displayName,
                EmailVerified = true,
                Disabled      = false
            }, ct);

            logger.LogInformation("Created Firebase user (uid={Uid}).", created.Uid);
            return created;
        }
    }
}
