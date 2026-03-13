using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateInventoryItem;

public sealed record UpdateInventoryItemCommand : IRequest<OrganizationInventoryResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    [JsonIgnore]
    public Guid Id { get; init; }
    [JsonIgnore]
    public Guid OrgId { get; init; }
    public string? ItemName { get; init; }
    public string? Description { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string[]? ImageUrls { get; init; }
    public string? StorageLocation { get; init; }
    public string? Status { get; init; }
}
