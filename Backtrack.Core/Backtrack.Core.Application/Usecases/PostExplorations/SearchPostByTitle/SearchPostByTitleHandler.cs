using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchPostByTitle;

public sealed class SearchPostByTitleHandler(IPostRepository postRepository)
    : IRequestHandler<SearchPostByTitleQuery, IEnumerable<SearchPostResult>>
{
    public async Task<IEnumerable<SearchPostResult>> Handle(
        SearchPostByTitleQuery query,
        CancellationToken cancellationToken)
    {
        var posts = await postRepository.SearchByTitleAsync(query.Query, query.Filters, cancellationToken);

        return posts.Select(p => new SearchPostResult
        {
            Id                      = p.Id,
            Author                  = p.Author?.ToPostAuthorResult(),
            Organization            = p.Organization?.ToOrganizationOnPost(),
            PostType                = p.PostType,
            PostTitle               = p.PostTitle,
            Category                = p.Category,
            SubcategoryId           = p.SubcategoryId,
            PersonalBelongingDetail = p.PersonalBelongingDetail,
            CardDetail              = p.CardDetail,
            ElectronicDetail        = p.ElectronicDetail,
            OtherDetail             = p.OtherDetail,
            ImageUrls               = p.ImageUrls,
            Location                = p.Location,
            ExternalPlaceId         = p.ExternalPlaceId,
            DisplayAddress          = p.DisplayAddress,
            EventTime               = p.EventTime,
            CreatedAt               = p.CreatedAt,
            Score                   = null,
            DistanceInMeters        = null
        });
    }
}
