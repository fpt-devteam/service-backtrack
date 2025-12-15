using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.GetPosts;

public sealed class GetPostsHandler : IRequestHandler<GetPostsQuery, PagedResult<PostResult>>
{
    private readonly IPostRepository _postRepository;

    public GetPostsHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PagedResult<PostResult>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _postRepository.GetPagedAsync(
            offset: request.PagedQuery.Offset,
            limit: request.PagedQuery.Limit,
            postType: request.PostType,
            searchTerm: request.SearchTerm,
            latitude: request.Latitude,
            longitude: request.Longitude,
            radiusInKm: request.RadiusInKm,
            cancellationToken: cancellationToken);

        GeoPoint? searchPoint = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            searchPoint = new GeoPoint(request.Latitude.Value, request.Longitude.Value);
        }

        var postResults = items.Select(post =>
        {
            double? distanceInKm = null;
            //if (searchPoint != null && post.Location != null)
            //{
            //    distanceInKm = searchPoint.DistanceToInKilometers(post.Location);
            //}

            return new PostResult
            {
                Id = post.Id,
                PostType = post.PostType.ToString(),
                ItemName = post.ItemName,
                Description = post.Description,
                Material = post.Material,
                Brands = post.Brands,
                Colors = post.Colors,
                ImageUrls = post.ImageUrls,
                Location = post.Location != null
                    ? new LocationResult
                    {
                        Latitude = post.Location.Latitude,
                        Longitude = post.Location.Longitude
                    }
                    : null,
                EventTime = post.EventTime,
                CreatedAt = post.CreatedAt,
                DistanceInKm = distanceInKm
            };
        }).ToList();

        return new PagedResult<PostResult>(totalCount, postResults);
    }
}
