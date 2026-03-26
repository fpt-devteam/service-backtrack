namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record OrganizationInventoryResult
{
    public required Guid Id { get; init; }
    public required Guid OrgId { get; init; }
    public required string LoggedById { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public string? StorageLocation { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset LoggedAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public FinderContactResult? FinderContact { get; init; }
}

public sealed record FinderContactResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? NationalId { get; init; }
    public string? OrgMemberId { get; init; }
}
