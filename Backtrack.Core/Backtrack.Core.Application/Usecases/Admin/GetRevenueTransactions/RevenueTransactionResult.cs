using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueTransactions;

public sealed record RevenueTransactionResult
{
    public required Guid            Id              { get; init; }
    public required string          RevenueType     { get; init; }
    public          string?         TenantId        { get; init; }
    public          string?         TenantName      { get; init; }
    public          string?         UserId          { get; init; }
    public          string?         UserName        { get; init; }
    public required decimal         Amount          { get; init; }
    public required string          Currency        { get; init; }
    public required string          Status          { get; init; }
    public required string          PaymentMethod   { get; init; }
    public required DateTimeOffset  TransactionDate { get; init; }
    public required string          Description     { get; init; }
    public          string?         InvoiceNumber   { get; init; }
    public          string?         SubscriptionPlan { get; init; }
}

public sealed record RevenueTransactionsPageResult(
    List<RevenueTransactionResult> Items,
    int                            Total);
