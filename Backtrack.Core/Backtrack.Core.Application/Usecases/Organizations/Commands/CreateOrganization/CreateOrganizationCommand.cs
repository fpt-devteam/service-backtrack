using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand : IRequest<OrganizationResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    [Required]
    public required string Name { get; init; }
    [Required]
    public required string Slug { get; init; }
    public string? Address { get; init; }
    [Required]
    public required string Phone { get; init; }
    [Required]
    public required string IndustryType { get; init; }
    [Required]
    public required string TaxIdentificationNumber { get; init; }
}
