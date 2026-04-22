using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.ListPostByFeed;

public sealed class ListPostByFeedHandler(IPostRepository postRepository)
    : IRequestHandler<ListPostByFeedQuery, ListPostByFeedResult>
{
    public async Task<ListPostByFeedResult> Handle(ListPostByFeedQuery query, CancellationToken cancellationToken)
    {
        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);
        var filters = new PostFilters
        {
            PostType = query.PostType,
            Status   = PostStatus.Active
        };
        var (items, _) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

        var grouped = items
            .Select(p => p.ToSearchPostResult(
                distanceInMeters: p.Location != null
                    ? GeoUtil.Haversine(query.Location, p.Location)
                    : null))
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        static List<SearchPostResult> Get(Dictionary<ItemCategory, List<SearchPostResult>> d, ItemCategory key)
            => d.TryGetValue(key, out var list) ? list : [];

        return new ListPostByFeedResult
        {
            PersonalBelongings = Get(grouped, ItemCategory.PersonalBelongings),
            Cards              = Get(grouped, ItemCategory.Cards),
            Electronics        = Get(grouped, ItemCategory.Electronics),
            Others             = Get(grouped, ItemCategory.Others),
        };
    }
}
