using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class QrLogoSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var codesWithoutLogo = await db.Set<QrCode>()
            .IgnoreQueryFilters()
            .Where(q => q.DeletedAt == null && q.LogoUrl == null)
            .ToListAsync(ct);

        if (codesWithoutLogo.Count == 0)
        {
            logger.LogInformation("QrLogoSeeder — all QR codes already have a logo, skipping.");
            return;
        }

        var rng = new Random();
        foreach (var qr in codesWithoutLogo)
            qr.LogoUrl = $"https://img.heroui.chat/image/avatar?w=400&h=400&u={rng.Next(1, 501)}";

        await db.SaveChangesAsync(ct);

        logger.LogInformation("QrLogoSeeder — assigned default logos to {Count} QR codes.", codesWithoutLogo.Count);
    }
}
