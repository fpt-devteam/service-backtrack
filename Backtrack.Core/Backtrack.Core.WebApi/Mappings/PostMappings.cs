using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using Backtrack.Core.Application.Posts.Commands.CreatePost;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Application.Posts.Queries.GetPosts;
using Backtrack.Core.Application.Posts.Queries.SearchPostsBySemantic;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.WebApi.Contracts.Posts.Requests;
using Backtrack.Core.WebApi.Contracts.Posts.Responses;

namespace Backtrack.Core.WebApi.Mappings
{
    public static class PostMappings
    {
        // ==================== Request to Command/Query ====================

        public static CreatePostCommand ToCommand(this CreatePostRequest request)
        {
            var postType = ParsePostType(request.PostType);

            return new CreatePostCommand
            {
                PostType = postType,
                ItemName = request.ItemName,
                Description = request.Description,
                ImageUrls = request.ImageUrls,
                Location = request.Location?.ToDto(),
                ExternalPlaceId = request.ExternalPlaceId,
                DisplayAddress = request.DisplayAddress,
                EventTime = request.EventTime
            };
        }

        public static GetPostsQuery ToQuery(this GetPostsRequest request)
        {
            PostType? postType = null;
            if (!string.IsNullOrWhiteSpace(request.PostType))
            {
                postType = ParsePostType(request.PostType);
            }

            return new GetPostsQuery(
                PagedQuery.FromPage(request.Page, request.PageSize),
                postType,
                request.SearchTerm,
                request.Latitude,
                request.Longitude,
                request.RadiusInKm
            );
        }

        public static SearchPostsBySemanticQuery ToQuery(this SearchPostsBySemanticRequest request)
        {
            PostType? postType = null;
            if (!string.IsNullOrWhiteSpace(request.PostType))
            {
                postType = ParsePostType(request.PostType);
            }

            return new SearchPostsBySemanticQuery(
                SearchText: request.SearchText,
                PagedQuery: PagedQuery.FromPage(request.Page, request.PageSize),
                PostType: postType,
                Latitude: request.Latitude,
                Longitude: request.Longitude,
                RadiusInKm: request.RadiusInKm
            );
        }

        // ==================== Result to Response ====================

        public static PostResponse ToResponse(this PostResult result)
        {
            return new PostResponse
            {
                Id = result.Id,
                PostType = result.PostType,
                ItemName = result.ItemName,
                Description = result.Description,
                ImageUrls = result.ImageUrls,
                Location = result.Location?.ToResponse(),
                ExternalPlaceId = result.ExternalPlaceId,
                DisplayAddress = result.DisplayAddress,
                EventTime = result.EventTime,
                CreatedAt = result.CreatedAt
            };
        }

        public static PostSemanticSearchResponse ToResponse(this PostSemanticSearchResult result)
        {
            return new PostSemanticSearchResponse
            {
                Id = result.Id,
                PostType = result.PostType,
                ItemName = result.ItemName,
                Description = result.Description,
                ImageUrls = result.ImageUrls,
                Location = result.Location?.ToResponse(),
                ExternalPlaceId = result.ExternalPlaceId,
                DisplayAddress = result.DisplayAddress,
                EventTime = result.EventTime,
                CreatedAt = result.CreatedAt,
                SimilarityScore = result.SimilarityScore
            };
        }

        // ==================== Location Mappings ====================

        public static LocationDto ToDto(this LocationRequest request)
        {
            return new LocationDto
            {
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };
        }

        public static LocationResponse ToResponse(this LocationResult result)
        {
            return new LocationResponse
            {
                Latitude = result.Latitude,
                Longitude = result.Longitude
            };
        }

        // ==================== Parsers ====================

        private static PostType ParsePostType(string postTypeString)
        {
            if (!Enum.TryParse<PostType>(postTypeString, ignoreCase: true, out var postType))
            {
                throw new ValidationException(PostErrors.InvalidPostType);
            }

            return postType;
        }
    }
}
