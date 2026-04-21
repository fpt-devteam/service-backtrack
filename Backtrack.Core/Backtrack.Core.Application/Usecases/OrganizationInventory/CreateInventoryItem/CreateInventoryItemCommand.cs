using System.Text.Json.Serialization;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.CreateInventoryItem;

public sealed record CreateInventoryItemCommand : IRequest<InventoryItemResult>
{
    [JsonIgnore] public string StaffId { get; init; } = string.Empty;
    [JsonIgnore] public Guid OrgId { get; init; }
    public required string PostTitle { get; init; }
    public required string Category { get; init; }
    public required string SubcategoryCode { get; init; }
    public PersonalBelongingDetailDto? PersonalBelongingDetail { get; init; }
    public CardDetailDto? CardDetail { get; init; }
    public ElectronicDetailDto? ElectronicDetail { get; init; }
    public OtherDetailDto? OtherDetail { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public required string InternalLocation { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required FinderInfo FinderInfo { get; init; }
}
