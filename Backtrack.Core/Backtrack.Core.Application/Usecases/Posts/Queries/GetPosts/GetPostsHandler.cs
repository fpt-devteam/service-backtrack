using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.Posts.Queries.GetPosts;

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
        PostType? postType = null;
        if (!string.IsNullOrWhiteSpace(query.PostType))
        {
            if (!Enum.TryParse<PostType>(query.PostType, ignoreCase: true, out var parsed))
            {
                throw new ValidationException(PostErrors.InvalidPostType);
            }
            postType = parsed;
        }

        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);

        var (items, totalCount) = await _postRepository.GetPagedAsync(
            offset: pagedQuery.Offset,
            limit: pagedQuery.Limit,
            postType: postType,
            searchTerm: query.SearchTerm,
            latitude: query.Latitude,
            longitude: query.Longitude,
            radiusInKm: query.RadiusInKm,
            authorId: query.AuthorId,
            cancellationToken: cancellationToken);

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
