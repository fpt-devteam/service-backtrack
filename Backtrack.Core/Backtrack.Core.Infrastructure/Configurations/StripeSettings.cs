namespace Backtrack.Core.Infrastructure.Configurations;

public sealed class StripeSettings
{
    public required string SecretKey { get; init; }
    public required string WebhookSecret { get; init; }
}
