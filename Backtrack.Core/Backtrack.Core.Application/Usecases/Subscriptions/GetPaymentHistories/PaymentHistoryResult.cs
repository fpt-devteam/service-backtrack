using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Subscriptions.GetPaymentHistories;

public sealed record PaymentHistoryResult
{
    public required Guid Id { get; init; }
    public required Guid SubscriptionId { get; init; }
    public required SubscriberType SubscriberType { get; init; }
    public string? UserId { get; init; }
    public Guid? OrganizationId { get; init; }
    public required string ProviderInvoiceId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required PaymentStatus Status { get; init; }
    public required DateTimeOffset PaymentDate { get; init; }
    public string? InvoiceUrl { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
