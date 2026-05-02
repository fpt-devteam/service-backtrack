namespace Backtrack.Core.Infrastructure.Configurations;

public sealed class StripeSettings
{
    public required string SecretKey { get; init; }
    public required string WebhookSecret { get; init; }

    // Subscription plan price IDs (Stripe Price IDs)
    public string? UserMonthlyPriceId { get; init; }
    public string? UserYearlyPriceId { get; init; }
    public string? OrgMonthlyPriceId { get; init; }
    public string? OrgYearlyPriceId { get; init; }
}
