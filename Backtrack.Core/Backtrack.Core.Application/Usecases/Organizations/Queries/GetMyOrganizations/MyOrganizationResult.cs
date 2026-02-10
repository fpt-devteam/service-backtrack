namespace Backtrack.Core.Application.Usecases.Organizations.Queries.GetMyOrganizations;

public sealed record MyOrganizationResult
{
    public required Guid OrgId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Address { get; init; }
    public required string Phone { get; init; }
    public required string IndustryType { get; init; }
    public required string TaxIdentificationNumber { get; init; }
    public required string OrgStatus { get; init; }
    public required string MyRole { get; init; }
    public required DateTimeOffset JoinedAt { get; init; }
}
