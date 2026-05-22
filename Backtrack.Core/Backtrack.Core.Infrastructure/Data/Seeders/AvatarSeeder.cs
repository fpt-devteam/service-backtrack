using Backtrack.Core.Application.Usecases.Users.UpdateUserProfile;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class AvatarSeeder
{
    private static string Avatar(int u) => $"https://img.heroui.chat/image/avatar?w=400&h=400&u={u}";

    public static async Task SeedAsync(ApplicationDbContext db, ISender mediator, ILogger logger, CancellationToken ct = default)
    {
        var users = await db.Set<User>()
            .IgnoreQueryFilters()
            .Where(u => u.Email != null && u.Email != "" && (u.AvatarUrl == null || u.AvatarUrl == ""))
            .Select(u => u.Id)
            .ToListAsync(ct);

        if (users.Count == 0)
        {
            logger.LogInformation("AvatarSeeder — no users without avatars found, skipping.");
            return;
        }

        var rng = new Random();
        var updated = new List<string>();

        foreach (var userId in users)
        {
            try
            {
                await mediator.Send(new UpdateUserProfileCommand
                {
                    UserId    = userId,
                    AvatarUrl = Avatar(rng.Next(1, 501))
                }, ct);

                updated.Add(userId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AvatarSeeder — failed to update avatar for user {UserId}, skipping.", userId);
            }
        }

        logger.LogInformation("AvatarSeeder — assigned random avatars to {Count}/{Total} users.", updated.Count, users.Count);
    }
}
