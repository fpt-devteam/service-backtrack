using System.Text.Json.Serialization;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.UpdateInventoryItem;

public sealed record UpdateInventoryItemCommand : IRequest<InventoryItemResult>
{
    [JsonIgnore] public Guid PostId { get; init; }
    [JsonIgnore] public string UserId { get; init; } = string.Empty;
    [JsonIgnore] public Guid OrgId { get; init; }
    public string? PostTitle { get; init; }
    public string? Status { get; init; }
    public string? InternalLocation { get; init; }
    public PersonalBelongingDetailDto? PersonalBelongingDetail { get; init; }
    public CardDetailDto? CardDetail { get; init; }
    public ElectronicDetailDto? ElectronicDetail { get; init; }
    public OtherDetailDto? OtherDetail { get; init; }
    public string[]? ImageUrls { get; init; }
    public DateTimeOffset? EventTime { get; init; }
}
