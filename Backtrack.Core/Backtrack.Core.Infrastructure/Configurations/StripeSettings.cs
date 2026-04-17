namespace Backtrack.Core.Infrastructure.Configurations;

public sealed class StripeSettings
{
    public required string SecretKey { get; init; }
    public required string WebhookSecret { get; init; }

    // Subscription plan price IDs (Stripe Price IDs)
    public string? UserMonthlyPriceId { get; init; }
    public string? UserYearlyPriceId { get; init; }
    public string? OrgFreePriceId { get; init; }
    public string? OrgProPriceId { get; init; }
    public string? OrgMaxPriceId { get; init; }
}
