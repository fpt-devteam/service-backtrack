using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.PostExplorations;

public static class SearchPostResultMapper
{
    public static SearchPostResult ToSearchPostResult(
        this Post post,
        double? score = null,
        double? distanceInMeters = null) => new()
    {
        Id                      = post.Id,
        Author                  = post.Author?.ToPostAuthorResult(),
        Organization            = post.Organization?.ToOrganizationOnPost(),
        PostType                = post.PostType,
        PostTitle               = post.PostTitle,
        Category                = post.Category,
        SubcategoryId           = post.SubcategoryId,
        PersonalBelongingDetail = post.PersonalBelongingDetail?.ToDto(),
        CardDetail              = post.CardDetail?.ToDto(),
        ElectronicDetail        = post.ElectronicDetail?.ToDto(),
        OtherDetail             = post.OtherDetail?.ToDto(),
        ImageUrls               = post.ImageUrls,
        Location                = post.Location,
        ExternalPlaceId         = post.ExternalPlaceId,
        DisplayAddress          = post.DisplayAddress,
        EventTime               = post.EventTime,
        CreatedAt               = post.CreatedAt,
        Score                   = score,
        DistanceInMeters        = distanceInMeters,
    };
}
