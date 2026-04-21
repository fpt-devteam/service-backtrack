using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.FullTextSearchPost;

/// <remarks>
/// Full-text search (tsvector/BM25) has been removed. This handler now delegates
/// to semantic (embedding) search via GetPagedAsync as a fallback.
/// Use SemanticSearchPost for proper embedding-based search.
/// </remarks>
public sealed class FullTextSearchPostHandler(IPostRepository postRepository)
    : IRequestHandler<FullTextSearchPostCommand, IEnumerable<SearchPostResult>>
{
    public async Task<IEnumerable<SearchPostResult>> Handle(FullTextSearchPostCommand command, CancellationToken cancellationToken)
    {
        var searchLocation = command.Filters?.Geo?.Location;

        // Full-text search via tsvector removed — fall back to paged listing with filters
        var (items, _) = await postRepository.GetPagedAsync(PagedQuery.Default, command.Filters, cancellationToken);

        return items.Select(post => new SearchPostResult
        {
            Id               = post.Id,
            Author           = post.Author?.ToPostAuthorResult(),
            Organization     = post.Organization?.ToOrganizationOnPost(),
            PostType         = post.PostType,
            PostTitle        = post.PostTitle,
            Category         = post.Category,
            SubcategoryId    = post.SubcategoryId,
            PersonalBelongingDetail = post.PersonalBelongingDetail,
            CardDetail       = post.CardDetail,
            ElectronicDetail = post.ElectronicDetail,
            OtherDetail      = post.OtherDetail,
            ImageUrls        = post.ImageUrls,
            Location         = post.Location,
            ExternalPlaceId  = post.ExternalPlaceId,
            DisplayAddress   = post.DisplayAddress,
            EventTime        = post.EventTime,
            CreatedAt        = post.CreatedAt,
            DistanceInMeters = searchLocation != null && post.Location != null
                ? GeoUtil.Haversine(searchLocation, post.Location)
                : null
        });
    }
}
