using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Posts.Queries.GetPosts;

public sealed class GetPostsHandler : IRequestHandler<GetPostsQuery, PagedResult<PostResult>>
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<GetPostsHandler> _logger;

    public GetPostsHandler(IPostRepository postRepository, ILogger<GetPostsHandler> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    public async Task<PagedResult<PostResult>> Handle(GetPostsQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _postRepository.GetPagedAsync(
            offset: query.PagedQuery.Offset,
            limit: query.PagedQuery.Limit,
            postType: query.PostType,
            searchTerm: query.SearchTerm,
            latitude: query.Latitude,
            longitude: query.Longitude,
            radiusInKm: query.RadiusInKm,
            authorId: query.AuthorId,
            cancellationToken: cancellationToken);

        GeoPoint? searchPoint = null;
        if (query.Latitude.HasValue && query.Longitude.HasValue)
        {
            searchPoint = new GeoPoint(query.Latitude.Value, query.Longitude.Value);
        }

        var postResults = items.Select(post =>
        {
            return new PostResult
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                Author = new AuthorResult
                {
                    Id = post.Author.Id,
                    DisplayName = post.Author.DisplayName,
                    AvatarUrl = post.Author.AvatarUrl
                },
                PostType = post.PostType.ToString(),
                ItemName = post.ItemName,
                Description = post.Description,
                ImageUrls = post.ImageUrls,
                Location = post.Location != null
                    ? new LocationResult
                    {
                        Latitude = post.Location.Latitude,
                        Longitude = post.Location.Longitude
                    }
                    : null,
                ExternalPlaceId = post.ExternalPlaceId,
                DisplayAddress = post.DisplayAddress,
                EventTime = post.EventTime,
                CreatedAt = post.CreatedAt
            };
        }).ToList();

        return new PagedResult<PostResult>(totalCount, postResults);
    }
}
