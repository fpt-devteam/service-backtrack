using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.GetDashboardInventory;

public sealed class GetDashboardInventoryHandler(
    IMembershipRepository      membershipRepository,
    IPostRepository            postRepository,
    IOrgReturnReportRepository returnReportRepository,
    ISubcategoryRepository     subcategoryRepository)
    : IRequestHandler<GetDashboardInventoryQuery, PagedResult<DashboardInventoryItem>>
{
    public async Task<PagedResult<DashboardInventoryItem>> Handle(
        GetDashboardInventoryQuery query, CancellationToken cancellationToken)
    {
        _ = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var authorId = string.Equals(query.StaffId, "me", StringComparison.OrdinalIgnoreCase)
            ? query.UserId
            : null;

        var filters = new PostFilters
        {
            OrganizationId = query.OrgId,
            PostType       = PostType.Found,
            AuthorId       = authorId
        };

        var (posts, totalCount) = await postRepository.GetPagedAsync(
            PagedQuery.FromPage(query.Page, query.PageSize), filters, cancellationToken);

        var postList    = posts.ToList();
        var postIds     = postList.Select(p => p.Id).ToList();
        var returnReports   = await returnReportRepository.GetByPostIdsAsync(postIds, cancellationToken);
        var subcategories   = (await subcategoryRepository.GetAllActiveAsync(cancellationToken))
                                  .ToDictionary(s => s.Id, s => s.Name);

        var items = postList.Select(p =>
        {
            returnReports.TryGetValue(p.Id, out var returnReport);
            var status = returnReport is not null ? "ReturnScheduled" : p.Status.ToString();
            subcategories.TryGetValue(p.SubcategoryId, out var subcategoryName);

            return new DashboardInventoryItem
            {
                Id               = p.Id,
                PostTitle        = p.PostTitle,
                Category         = p.Category.ToString(),
                SubcategoryName  = subcategoryName ?? string.Empty,
                Status           = status,
                InternalLocation = p.InternalLocation ?? string.Empty,
                ImageUrl         = p.ImageUrls.FirstOrDefault(),
                CreatedAt        = p.CreatedAt
            };
        }).ToList();

        return new PagedResult<DashboardInventoryItem>(totalCount, items);
    }
}
