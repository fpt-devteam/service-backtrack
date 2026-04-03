using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchPosts;

public sealed class SearchPostsHandler(IPostRepository postRepository)
    : IRequestHandler<SearchPostsCommand, IEnumerable<SearchPostResult>>
{
    public async Task<IEnumerable<SearchPostResult>> Handle(SearchPostsCommand command, CancellationToken cancellationToken)
    {
        var posts = await postRepository.SearchByFullTextAsync(
            command.Query,
            command.Filters,
            cancellationToken);

        var searchLocation = command.Filters?.Geo?.Location;

        return posts.Select(post => new SearchPostResult
        {
            Id              = post.Id,
            Author          = post.Author?.ToPostAuthorResult(),
            Organization    = post.Organization?.ToOrganizationOnPost(),
            PostType        = post.PostType,
            Item            = post.Item,
            ImageUrls       = post.ImageUrls,
            Location        = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress  = post.DisplayAddress,
            EventTime       = post.EventTime,
            CreatedAt       = post.CreatedAt,
            DistanceInMeters = searchLocation != null && post.Location != null
                ? GeoUtil.Haversine(searchLocation, post.Location)
                : null
        });
    }
}
