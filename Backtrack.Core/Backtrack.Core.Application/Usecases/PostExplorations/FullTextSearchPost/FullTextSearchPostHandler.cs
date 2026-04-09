using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.FullTextSearchPost;

public sealed class FullTextSearchPostHandler(IPostRepository postRepository)
    : IRequestHandler<FullTextSearchPostCommand, IEnumerable<SearchPostResult>>
{
    public async Task<IEnumerable<SearchPostResult>> Handle(FullTextSearchPostCommand command, CancellationToken cancellationToken)
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
