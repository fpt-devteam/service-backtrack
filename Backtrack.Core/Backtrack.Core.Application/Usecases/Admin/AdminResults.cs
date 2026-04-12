using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Application.Usecases.Subscriptions;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Users;

namespace Backtrack.Core.Application.Usecases.Admin;

public sealed record AdminUserSummaryResult
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public required UserStatus Status { get; init; }
    public required UserGlobalRole GlobalRole { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed record AdminUserDetailResult
{
    public required UserResult BasicInfo { get; init; }
    public SubscriptionResult? Subscription { get; init; }
    public required QrUsageOverview QrUsage { get; init; }
    public required List<PaymentHistoryResult> BillingHistory { get; init; }
}

public sealed record QrUsageOverview
{
    public required int TotalQrCodes { get; init; }
}

public sealed record AdminOrgSummaryResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? LogoUrl { get; init; }
    public required OrganizationStatus Status { get; init; }
    public required int MemberCount { get; init; }
    public required int PostCount { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed record AdminOrgDetailResult
{
    public required OrganizationResult BasicInfo { get; init; }
    public SubscriptionResult? Subscription { get; init; }
    public required OrgUsageOverview UsageOverview { get; init; }
    public required List<PaymentHistoryResult> BillingHistory { get; init; }
}

public sealed record OrgUsageOverview
{
    public required int MemberCount { get; init; }
    public required int TotalPostCount { get; init; }
    public required int ActivePostCount { get; init; }
}

public sealed record PaymentHistoryResult
{
    public required Guid Id { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required PaymentStatus Status { get; init; }
    public required DateTimeOffset PaymentDate { get; init; }
    public required string ProviderInvoiceId { get; init; }
    public string? PlanName { get; init; }
}
