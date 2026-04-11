using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Infrastructure.Configurations;
using Backtrack.Core.Infrastructure.Payments;
using Backtrack.Core.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backtrack.Core.Infrastructure.DependencyInjections;

public static class StripeDI
{
    public static void AddStripe(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StripeSettings>(configuration.GetSection("StripeSettings"));
        services.AddSingleton<IStripeService, StripeService>();

        services.AddScoped<IQrCodeRepository, QrCodeRepository>();
        services.AddScoped<IQrDesignRepository, QrDesignRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentHistoryRepository, PaymentHistoryRepository>();
    }
}
