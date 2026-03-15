using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Organizations.SearchInventoryBySemantic;

public sealed record SearchInventoryBySemanticQuery : IRequest<PagedResult<InventorySemanticSearchResult>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public required Guid OrgId { get; init; }
    public required string SearchText { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
