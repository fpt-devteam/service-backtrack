using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class PaymentHistory : Entity<Guid>
{
    public required Guid SubscriptionId { get; set; }
    public required SubscriberType SubscriberType { get; set; }
    public string? UserId { get; set; }
    public Guid? OrganizationId { get; set; }

    public required string ProviderInvoiceId { get; set; } // Stripe invoice ID
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required PaymentStatus Status { get; set; }
    public required DateTimeOffset PaymentDate { get; set; }
    public string? InvoiceUrl { get; set; }

    public Subscription Subscription { get; set; } = default!;
}
