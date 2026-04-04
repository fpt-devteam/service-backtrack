using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Organizations.GetInventoryItems;

public sealed record GetInventoryItemsQuery : IRequest<PagedResult<OrganizationInventoryResult>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public required Guid OrgId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? Status { get; init; }
}
