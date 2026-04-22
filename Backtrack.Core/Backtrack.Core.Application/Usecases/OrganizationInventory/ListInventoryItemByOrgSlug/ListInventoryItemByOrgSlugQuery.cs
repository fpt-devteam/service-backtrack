using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.ListInventoryItemByOrgSlug;

public sealed record ListInventoryItemByOrgSlugQuery : IRequest<PagedResult<PostResult>>
{
    public required string Slug { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
