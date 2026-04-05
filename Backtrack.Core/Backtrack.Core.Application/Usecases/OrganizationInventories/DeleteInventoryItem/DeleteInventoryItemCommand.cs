using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.DeleteInventoryItem;

public sealed record DeleteInventoryItemCommand : IRequest<Unit>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public required Guid Id { get; init; }
    public required Guid OrgId { get; init; }
}
