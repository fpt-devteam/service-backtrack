namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record OrganizationResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Address { get; init; }
    public required string Phone { get; init; }
    public required string IndustryType { get; init; }
    public required string TaxIdentificationNumber { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
