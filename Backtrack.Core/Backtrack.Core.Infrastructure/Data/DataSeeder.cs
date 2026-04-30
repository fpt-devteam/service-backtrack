using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Infrastructure.Configurations;
using Backtrack.Core.Infrastructure.Data.Seeders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        ISender mediator,
        IOrganizationRepository orgRepository,
        ISubscriptionRepository subscriptionRepository,
        ILogger logger,
        StripeSettings? stripeSettings = null,
        SuperAdminSettings? superAdminSettings = null,
        CancellationToken ct = default)
    {
        await SubcategorySeeder.SeedAsync(db, logger, ct);
        await SubscriptionPlanSeeder.SeedAsync(db, logger, stripeSettings, ct);
        await SuperAdminSeeder.SeedAsync(db, logger, superAdminSettings, ct);
        await UserSeeder.SeedAsync(db, mediator, logger, ct);
        await UserSubscriptionSeeder.SeedAsync(db, logger, ct);
        await OrganizationSeeder.SeedAsync(db, mediator, orgRepository, subscriptionRepository, logger, ct);
        await PostSeeder.SeedAsync(db, mediator, logger, ct);
        // await OrgInventorySeeder.SeedAsync(db, mediator, logger, ct);
    }
}
