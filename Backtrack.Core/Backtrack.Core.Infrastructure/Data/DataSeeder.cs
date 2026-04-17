using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Configurations;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data;

public static class DataSeeder
{
    // ── Subcategory definitions ────────────────────────────────────────────────
    private static readonly (ItemCategory Category, string Code, string Name, int Order)[] SubcategoryDefinitions =
    [
        // Electronics
        (ItemCategory.Electronics, "phone",           "Phone",           1),
        (ItemCategory.Electronics, "laptop",          "Laptop",          2),
        (ItemCategory.Electronics, "smartwatch",      "Smartwatch",      3),
        (ItemCategory.Electronics, "charger_adapter", "Charger Adapter", 4),
        (ItemCategory.Electronics, "mouse",           "Mouse",           5),
        (ItemCategory.Electronics, "keyboard",        "Keyboard",        6),
        (ItemCategory.Electronics, "powerbank",       "Powerbank",       7),
        (ItemCategory.Electronics, "power_outlet",    "Power Outlet",    8),
        (ItemCategory.Electronics, "headphone",       "Headphone",       9),
        (ItemCategory.Electronics, "earphone",        "Earphone",        10),

        // Cards
        (ItemCategory.Cards, "identification_card", "Identification Card", 1),
        (ItemCategory.Cards, "passport",            "Passport",            2),
        (ItemCategory.Cards, "driver_license",      "Driver License",      3),
        (ItemCategory.Cards, "personal_card",       "Personal Card",       4),
        (ItemCategory.Cards, "bank_card",           "Bank Card",           5),
        (ItemCategory.Cards, "student_card",        "Student Card",        6),
        (ItemCategory.Cards, "company_card",        "Company Card",        7),

        // PersonalBelongings
        (ItemCategory.PersonalBelongings, "wallets",   "Wallets",   1),
        (ItemCategory.PersonalBelongings, "keys",      "Keys",      2),
        (ItemCategory.PersonalBelongings, "suitcases", "Suitcases", 3),
        (ItemCategory.PersonalBelongings, "backpack",  "Backpack",  4),
        (ItemCategory.PersonalBelongings, "clothings", "Clothings", 5),
        (ItemCategory.PersonalBelongings, "jewelry",   "Jewelry",   6),
    ];

    // ── Entry point ───────────────────────────────────────────────────────────
    public static async Task SeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        StripeSettings? stripeSettings = null,
        SuperAdminSettings? superAdminSettings = null,
        CancellationToken ct = default)
    {
        await SeedSubcategoriesAsync(db, logger, ct);
        await SeedSubscriptionPlansAsync(db, logger, stripeSettings, ct);
        await SeedSuperAdminAsync(db, logger, superAdminSettings, ct);
    }

    // ── Subcategories ─────────────────────────────────────────────────────────
    private static async Task SeedSubcategoriesAsync(
        ApplicationDbContext db, ILogger logger, CancellationToken ct)
    {
        var hasAny = await db.Set<Subcategory>()
            .IgnoreQueryFilters()
            .AnyAsync(ct);

        if (hasAny)
        {
            logger.LogInformation("Subcategories already seeded — skipping.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var subcategories = SubcategoryDefinitions
            .Select(d => new Subcategory
            {
                Id           = Guid.NewGuid(),
                Category     = d.Category,
                Code         = d.Code,
                Name         = d.Name,
                DisplayOrder = d.Order,
                IsActive     = true,
                CreatedAt    = now
            })
            .ToList();

        db.Set<Subcategory>().AddRange(subcategories);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seeded {Count} subcategories.", subcategories.Count);
    }

    // ── Subscription plans ────────────────────────────────────────────────────
    private static async Task SeedSubscriptionPlansAsync(
        ApplicationDbContext db, ILogger logger, StripeSettings? stripe, CancellationToken ct)
    {
        if (stripe is null ||
            stripe.UserMonthlyPriceId is null ||
            stripe.UserYearlyPriceId  is null ||
            stripe.OrgFreePriceId     is null ||
            stripe.OrgProPriceId      is null ||
            stripe.OrgMaxPriceId      is null)
        {
            logger.LogInformation(
                "Stripe price IDs not fully configured — subscription plan seeding skipped.");
            return;
        }

        var definitions = BuildSubscriptionPlanDefinitions(stripe);
        var created = new List<string>();
        var skipped = 0;

        foreach (var plan in definitions)
        {
            var exists = await db.Set<SubscriptionPlan>()
                .IgnoreQueryFilters()
                .AnyAsync(p => p.ProviderPriceId == plan.ProviderPriceId, ct);

            if (exists)
            {
                skipped++;
                continue;
            }

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
            logger.LogInformation(
                "Subscription plans already seeded ({Skipped} skipped) — skipping.", skipped);
        }
    }

    private static List<SubscriptionPlan> BuildSubscriptionPlanDefinitions(StripeSettings stripe)
    {
        var now = DateTimeOffset.UtcNow;
        return
        [
            // ── User plans (QR Code activation) ──────────────────────────
            new SubscriptionPlan
            {
                Id              = Guid.NewGuid(),
                Name            = "QR Monthly",
                Price           = 1.99m,
                Currency        = "usd",
                BillingInterval = SubscriptionBillingInterval.Monthly,
                SubscriberType  = SubscriberType.User,
                ProviderPriceId = stripe.UserMonthlyPriceId!,
                Features        =
                [
                    "Activate your personal QR code",
                    "Custom note for finders",
                    "Personalized QR design",
                ],
                CreatedAt = now,
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
                Features        =
                [
                    "Activate your personal QR code",
                    "Custom note for finders",
                    "Personalized QR design",
                    "2 months free vs monthly",
                ],
                CreatedAt = now,
            },

            // ── Organization plans ────────────────────────────────────────
            new SubscriptionPlan
            {
                Id              = Guid.NewGuid(),
                Name            = "Org Free",
                Price           = 0m,
                Currency        = "usd",
                BillingInterval = SubscriptionBillingInterval.Monthly,
                SubscriberType  = SubscriberType.Organization,
                ProviderPriceId = stripe.OrgFreePriceId!,
                Features        =
                [
                    "Up to 3 staff members",
                    "Basic lost & found management",
                    "Public organization profile",
                ],
                CreatedAt = now,
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
                Features        =
                [
                    "Up to 20 staff members",
                    "Advanced lost & found management",
                    "AI-powered item matching",
                    "Custom intake forms",
                    "Analytics dashboard",
                ],
                CreatedAt = now,
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
                Features        =
                [
                    "Unlimited staff members",
                    "Advanced lost & found management",
                    "AI-powered item matching",
                    "Custom intake forms",
                    "Analytics dashboard",
                    "Priority support",
                    "Custom branding",
                ],
                CreatedAt = now,
            },
        ];
    }

    // ── Super-admin ───────────────────────────────────────────────────────────
    private static async Task SeedSuperAdminAsync(
        ApplicationDbContext db, ILogger logger, SuperAdminSettings? settings, CancellationToken ct)
    {
        if (settings is null)
        {
            logger.LogInformation("SuperAdminSettings not configured — super-admin seeding skipped.");
            return;
        }

        string firebaseUid;
        try
        {
            var existing = await FirebaseAuth.DefaultInstance
                .GetUserByEmailAsync(settings.Email, ct);

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
