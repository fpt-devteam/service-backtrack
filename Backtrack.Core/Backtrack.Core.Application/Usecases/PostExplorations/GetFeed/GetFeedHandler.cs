using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.GetFeed;

public sealed class GetFeedHandler(IPostRepository postRepository)
    : IRequestHandler<GetFeedQuery, FeedPostResult>
{
    public async Task<FeedPostResult> Handle(GetFeedQuery query, CancellationToken cancellationToken)
    {
        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);
        var filters = new PostFilters
        {
            PostType = query.PostType,
            Status = Domain.Constants.PostStatus.Active
        };
        var (items, _) = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

        var grouped = items
            .Select(post => new FeedPostItem
            {
                Id = post.Id,
                Author = post.Author?.ToPostAuthorResult(),
                Organization = post.Organization?.ToOrganizationOnPost(),
                PostType = post.PostType,
                PostTitle = post.PostTitle,
                Category = post.Category,
                SubcategoryId = post.SubcategoryId,
                PersonalBelongingDetail = post.PersonalBelongingDetail,
                CardDetail = post.CardDetail,
                ElectronicDetail = post.ElectronicDetail,
                OtherDetail = post.OtherDetail,
                ImageUrls = post.ImageUrls,
                Location = post.Location!,
                ExternalPlaceId = post.ExternalPlaceId,
                DisplayAddress = post.DisplayAddress,
                EventTime = post.EventTime,
                CreatedAt = post.CreatedAt,
                DistanceInMeters = post.Location != null
                    ? GeoUtil.Haversine(query.Location, post.Location)
                    : 0
            })
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        static List<FeedPostItem> Get(Dictionary<ItemCategory, List<FeedPostItem>> d, ItemCategory key)
            => d.TryGetValue(key, out var list) ? list : [];

        return new FeedPostResult
        {
            PersonalBelongings = Get(grouped, ItemCategory.PersonalBelongings),
            Cards              = Get(grouped, ItemCategory.Cards),
            Electronics        = Get(grouped, ItemCategory.Electronics),
            Others             = Get(grouped, ItemCategory.Others),
        };
    }
}
