namespace Backtrack.Core.Application.Usecases.Admin.GetOrganizations;

public sealed record OrganizationItemResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? LogoUrl { get; init; }
    public string? AdminEmail { get; init; }
    public string? SubscriptionPlan { get; init; }
    public required string Status { get; init; }
    public required OrganizationCapacityResult Capacity { get; init; }
    public required double Performance { get; init; }
    public required long TotalRevenue { get; init; }
    public required double SuccessRate { get; init; }
    public DateTimeOffset? NextBilling { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
