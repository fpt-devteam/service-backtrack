using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class UserSubscriptionSeeder
{
    // First 5 users from UserSeeder — alternating monthly/yearly for variety
    private static readonly (string Email, SubscriptionBillingInterval Interval)[] TargetUsers =
    [
        (UserSeeder.LongFpt.Email,     SubscriptionBillingInterval.Monthly),
        (UserSeeder.PhiLong.Email,     SubscriptionBillingInterval.Yearly),
        (UserSeeder.PhiLongKoko.Email, SubscriptionBillingInterval.Monthly),
        (UserSeeder.PhiLongTik.Email,  SubscriptionBillingInterval.Yearly),
        (UserSeeder.NhatThang.Email,   SubscriptionBillingInterval.Monthly),
    ];

    public static async Task SeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken ct = default)
    {
        // Load user-tier plans (QR Monthly / QR Yearly)
        var userPlans = await db.Set<SubscriptionPlan>()
            .IgnoreQueryFilters()
            .Where(p => p.SubscriberType == SubscriberType.User && p.IsActive)
            .ToListAsync(ct);

        if (userPlans.Count == 0)
        {
            logger.LogInformation("No active user subscription plans found — user subscription seeding skipped.");
            return;
        }

        var created = new List<string>();
        var skipped = new List<string>();

        foreach (var (email, interval) in TargetUsers)
        {
            try
            {
                var user = await db.Set<User>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Email == email, ct);

                if (user is null)
                {
                    logger.LogWarning("User {Email} not found in DB — skipping subscription seed.", email);
                    skipped.Add(email);
                    continue;
                }

                var alreadySubscribed = await db.Set<Subscription>()
                    .IgnoreQueryFilters()
                    .AnyAsync(s => s.UserId == user.Id && s.SubscriberType == SubscriberType.User
                                   && s.Status == SubscriptionStatus.Active, ct);

                if (alreadySubscribed)
                {
                    skipped.Add(email);
                    continue;
                }

                var plan = userPlans.FirstOrDefault(p => p.BillingInterval == interval)
                           ?? userPlans.First();

                var now        = DateTimeOffset.UtcNow;
                var periodEnd  = plan.BillingInterval == SubscriptionBillingInterval.Yearly
                    ? now.AddYears(1)
                    : now.AddMonths(1);

                var shortId = user.Id.Length >= 8 ? user.Id[..8] : user.Id;

                var subscription = new Subscription
                {
                    Id                     = Guid.NewGuid(),
                    SubscriberType         = SubscriberType.User,
                    UserId                 = user.Id,
                    OrganizationId         = null,
                    PlanId                 = plan.Id,
                    PlanSnapshot           = new SubscriptionPlanSnapshot
                    {
                        Name            = plan.Name,
                        Price           = plan.Price,
                        Currency        = plan.Currency,
                        BillingInterval = plan.BillingInterval,
                        Features        = plan.Features,
                    },
                    ProviderSubscriptionId = $"sub_seed_{shortId}",
                    ProviderCustomerId     = $"cus_seed_{shortId}",
                    Status                 = SubscriptionStatus.Active,
                    CurrentPeriodStart     = now,
                    CurrentPeriodEnd       = periodEnd,
                    CancelAtPeriodEnd      = false,
                    CreatedAt              = now,
                };

                db.Set<Subscription>().Add(subscription);
                created.Add(email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to seed subscription for {Email} — skipping.", email);
                skipped.Add(email);
            }
        }

        if (created.Count > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "User subscriptions — created {Created}, skipped {Skipped}. Created: [{CreatedNames}] Skipped: [{SkippedNames}]",
            created.Count, skipped.Count,
            string.Join(", ", created),
            string.Join(", ", skipped));
    }
}
