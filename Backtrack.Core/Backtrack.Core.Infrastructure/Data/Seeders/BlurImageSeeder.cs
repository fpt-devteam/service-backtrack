using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class BlurImageSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        IImageBlurService imageBlurService,
        ILogger logger,
        CancellationToken ct = default)
    {
        var posts = await db.Set<Post>()
            .IgnoreQueryFilters()
            .Where(p => p.DeletedAt == null
                     && p.ImageUrls.Count > 0
                     && p.BlurImageUrls.Count == 0)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);

        if (posts.Count == 0)
        {
            logger.LogInformation("BlurImageSeeder — no posts with missing blur images found, skipping.");
            return;
        }

        logger.LogInformation("BlurImageSeeder — processing {Total} post(s).", posts.Count);

        var processed = 0;
        foreach (var post in posts)
        {
            var blurUrls = await imageBlurService.GenerateAndUploadAsync(
                post.ImageUrls,
                $"posts/{post.Id}/blur",
                ct);

            post.BlurImageUrls = blurUrls;
            processed++;
            logger.LogInformation("BlurImageSeeder — [{Done}/{Total}] post {PostId}: {Count} blur image(s) generated.", processed, posts.Count, post.Id, blurUrls.Count);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("BlurImageSeeder — completed. {Done} post(s) updated.", processed);
    }
}
