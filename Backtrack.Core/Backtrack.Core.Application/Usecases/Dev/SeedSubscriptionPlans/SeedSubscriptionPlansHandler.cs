using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Dev.SeedSubscriptionPlans;

public sealed class SeedSubscriptionPlansHandler(ISubscriptionPlanRepository planRepository)
    : IRequestHandler<SeedSubscriptionPlansCommand, SeedSubscriptionPlansResult>
{
    public async Task<SeedSubscriptionPlansResult> Handle(
        SeedSubscriptionPlansCommand command, CancellationToken cancellationToken)
    {
        var definitions = BuildPlanDefinitions(command);

        var created = new List<string>();
        var skipped = 0;

        foreach (var plan in definitions)
        {
            var existing = await planRepository.GetByProviderPriceIdAsync(plan.ProviderPriceId, cancellationToken);
            if (existing is not null)
            {
                skipped++;
                continue;
            }

            await planRepository.CreateAsync(plan);
            created.Add(plan.Name);
        }

        if (created.Count > 0)
            await planRepository.SaveChangesAsync();

        return new SeedSubscriptionPlansResult
        {
            Created = created.Count,
            Skipped = skipped,
            CreatedPlanNames = created,
        };
    }

    private static List<SubscriptionPlan> BuildPlanDefinitions(SeedSubscriptionPlansCommand cmd) =>
    [
        // ── User plans (QR Code activation) ──────────────────────────────
        new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "QR Monthly",
            Price = 1.99m,
            Currency = "usd",
            BillingInterval = SubscriptionBillingInterval.Monthly,
            SubscriberType = SubscriberType.User,
            ProviderPriceId = cmd.UserMonthlyPriceId,
            Features =
            [
                "Activate your personal QR code",
                "Custom note for finders",
                "Personalized QR design",
            ],
        },
        new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "QR Yearly",
            Price = 19.99m,
            Currency = "usd",
            BillingInterval = SubscriptionBillingInterval.Yearly,
            SubscriberType = SubscriberType.User,
            ProviderPriceId = cmd.UserYearlyPriceId,
            Features =
            [
                "Activate your personal QR code",
                "Custom note for finders",
                "Personalized QR design",
                "2 months free vs monthly",
            ],
        },

        // ── Organization plans (features & member slots) ─────────────────
        new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Org Free",
            Price = 0m,
            Currency = "usd",
            BillingInterval = SubscriptionBillingInterval.Monthly,
            SubscriberType = SubscriberType.Organization,
            ProviderPriceId = cmd.OrgFreePriceId,
            Features =
            [
                "Up to 3 staff members",
                "Basic lost & found management",
                "Public organization profile",
            ],
        },
        new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Org Pro",
            Price = 19.99m,
            Currency = "usd",
            BillingInterval = SubscriptionBillingInterval.Monthly,
            SubscriberType = SubscriberType.Organization,
            ProviderPriceId = cmd.OrgProPriceId,
            Features =
            [
                "Up to 20 staff members",
                "Advanced lost & found management",
                "AI-powered item matching",
                "Custom intake forms",
                "Analytics dashboard",
            ],
        },
        new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Org Max",
            Price = 49.99m,
            Currency = "usd",
            BillingInterval = SubscriptionBillingInterval.Monthly,
            SubscriberType = SubscriberType.Organization,
            ProviderPriceId = cmd.OrgMaxPriceId,
            Features =
            [
                "Unlimited staff members",
                "Advanced lost & found management",
                "AI-powered item matching",
                "Custom intake forms",
                "Analytics dashboard",
                "Priority support",
                "Custom branding",
            ],
        },
    ];
}
