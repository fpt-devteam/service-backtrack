using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Organizations.CreateOrganization;

public sealed record CreateOrganizationCommand : IRequest<OrganizationResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required GeoPoint Location { get; init; }
    public required string DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public required string Phone { get; init; }
    public required string IndustryType { get; init; }
    public required string TaxIdentificationNumber { get; init; }
}
