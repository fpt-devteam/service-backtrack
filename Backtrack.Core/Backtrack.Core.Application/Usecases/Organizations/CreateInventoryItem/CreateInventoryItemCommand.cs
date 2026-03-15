using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.CreateInventoryItem;

public sealed record CreateInventoryItemCommand : IRequest<OrganizationInventoryResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    [JsonIgnore]
    public Guid OrgId { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public string? StorageLocation { get; init; }
}
