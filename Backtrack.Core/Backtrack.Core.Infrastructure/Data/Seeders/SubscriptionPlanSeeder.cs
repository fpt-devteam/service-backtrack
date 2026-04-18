using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class SubscriptionPlanSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        StripeSettings? stripe,
        CancellationToken ct = default)
    {
        if (stripe is null ||
            stripe.UserMonthlyPriceId is null ||
            stripe.UserYearlyPriceId  is null ||
            stripe.OrgFreePriceId     is null ||
            stripe.OrgProPriceId      is null ||
            stripe.OrgMaxPriceId      is null)
        {
            logger.LogInformation("Stripe price IDs not fully configured — subscription plan seeding skipped.");
            return;
        }

        var definitions = BuildDefinitions(stripe);
        var created = new List<string>();
        var skipped = 0;

        foreach (var plan in definitions)
        {
            var exists = await db.Set<SubscriptionPlan>()
                .IgnoreQueryFilters()
                .AnyAsync(p => p.ProviderPriceId == plan.ProviderPriceId, ct);

            if (exists) { skipped++; continue; }

            db.Set<SubscriptionPlan>().Add(plan);
            created.Add(plan.Name);
        }

        if (created.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation(
                "Seeded {Created} subscription plan(s), skipped {Skipped}. Plans: {Names}",
                created.Count, skipped, string.Join(", ", created));
        }
        else
        {
            logger.LogInformation("Subscription plans already seeded ({Skipped} skipped).", skipped);
        }
    }

    private static List<SubscriptionPlan> BuildDefinitions(StripeSettings stripe)
    {
        var now = DateTimeOffset.UtcNow;
        return
        [
            new SubscriptionPlan
            {
                Id              = Guid.NewGuid(),
                Name            = "QR Monthly",
                Price           = 1.99m,
                Currency        = "usd",
                BillingInterval = SubscriptionBillingInterval.Monthly,
                SubscriberType  = SubscriberType.User,
                ProviderPriceId = stripe.UserMonthlyPriceId!,
                Features        = ["Activate your personal QR code", "Custom note for finders", "Personalized QR design"],
                CreatedAt       = now,
            },
            new SubscriptionPlan
            {
                Id              = Guid.NewGuid(),
                Name            = "QR Yearly",
                Price           = 19.99m,
                Currency        = "usd",
                BillingInterval = SubscriptionBillingInterval.Yearly,
                SubscriberType  = SubscriberType.User,
                ProviderPriceId = stripe.UserYearlyPriceId!,
                Features        = ["Activate your personal QR code", "Custom note for finders", "Personalized QR design", "2 months free vs monthly"],
                CreatedAt       = now,
            },
            new SubscriptionPlan
            {
                Id              = Guid.NewGuid(),
                Name            = "Org Free",
                Price           = 0m,
                Currency        = "usd",
                BillingInterval = SubscriptionBillingInterval.Monthly,
                SubscriberType  = SubscriberType.Organization,
                ProviderPriceId = stripe.OrgFreePriceId!,
                Features        = ["Up to 3 staff members", "Basic lost & found management", "Public organization profile"],
                CreatedAt       = now,
            },
            new SubscriptionPlan
            {
                Id              = Guid.NewGuid(),
                Name            = "Org Pro",
                Price           = 19.99m,
                Currency        = "usd",
                BillingInterval = SubscriptionBillingInterval.Monthly,
                SubscriberType  = SubscriberType.Organization,
                ProviderPriceId = stripe.OrgProPriceId!,
                Features        = ["Up to 20 staff members", "Advanced lost & found management", "AI-powered item matching", "Custom intake forms", "Analytics dashboard"],
                CreatedAt       = now,
            },
            new SubscriptionPlan
            {
                Id              = Guid.NewGuid(),
                Name            = "Org Max",
                Price           = 49.99m,
                Currency        = "usd",
                BillingInterval = SubscriptionBillingInterval.Monthly,
                SubscriberType  = SubscriberType.Organization,
                ProviderPriceId = stripe.OrgMaxPriceId!,
                Features        = ["Unlimited staff members", "Advanced lost & found management", "AI-powered item matching", "Custom intake forms", "Analytics dashboard", "Priority support", "Custom branding"],
                CreatedAt       = now,
            },
        ];
    }
}
