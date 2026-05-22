using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.BackgroundJobs;

public class ExpirePostsAndReportsJob(
    ApplicationDbContext context,
    ILogger<ExpirePostsAndReportsJob> logger)
{
    public async Task ExecuteAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var expiredPosts = await context.Posts
            .Where(p => p.DeletedAt == null &&
                        (p.Status == PostStatus.Active || p.Status == PostStatus.InStorage) &&
                        p.ExpiredAt <= now)
            .ToListAsync();

        foreach (var post in expiredPosts)
            post.Status = PostStatus.Expired;

        var expiredReports = await context.C2CReturnReports
            .Where(r => r.DeletedAt == null &&
                        (r.Status == C2CReturnReportStatus.Ongoing || r.Status == C2CReturnReportStatus.Delivered) &&
                        r.ExpiresAt <= now)
            .ToListAsync();

        foreach (var report in expiredReports)
            report.Status = C2CReturnReportStatus.Expired;

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Expiry job completed: {PostCount} posts and {ReportCount} C2C return reports marked as expired.",
            expiredPosts.Count,
            expiredReports.Count);
    }
}
