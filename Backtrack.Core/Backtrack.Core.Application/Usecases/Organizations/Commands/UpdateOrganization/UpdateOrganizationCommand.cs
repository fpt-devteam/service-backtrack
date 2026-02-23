using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand : IRequest<OrganizationResult>
{
    public Guid OrgId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Address { get; init; }
    public required string Phone { get; init; }
    public required string IndustryType { get; init; }
    public required string TaxIdentificationNumber { get; init; }
}
