using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetFeed;

public sealed class GetFeedHandler(IPostRepository postRepository)
    : IRequestHandler<GetFeedQuery, PagedResult<FeedPostResult>>
{
    public async Task<PagedResult<FeedPostResult>> Handle(GetFeedQuery query, CancellationToken cancellationToken)
    {
        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);
        var (items, totalCount) = await postRepository.GetFeedAsync(query.Location, pagedQuery.Offset, pagedQuery.Limit, cancellationToken);

        var results = items.Select(item => new FeedPostResult
        {
            Id = item.Post.Id,
            Author = item.Post.Author?.ToPostAuthorResult(),
            Organization = item.Post.Organization?.ToOrganizationOnPost(),
            PostType = item.Post.PostType,
            Item = item.Post.Item,
            ImageUrls = item.Post.ImageUrls,
            Location = item.Post.Location,
            ExternalPlaceId = item.Post.ExternalPlaceId,
            DisplayAddress = item.Post.DisplayAddress,
            EventTime = item.Post.EventTime,
            CreatedAt = item.Post.CreatedAt,
            DistanceInMeters = item.DistanceMeters
        }).ToList();

        return new PagedResult<FeedPostResult>(totalCount, results);
    }
}
