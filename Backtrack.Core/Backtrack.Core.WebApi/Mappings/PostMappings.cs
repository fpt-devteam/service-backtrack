using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using Backtrack.Core.Application.Posts.Commands.CreatePost;
using Backtrack.Core.Application.Posts.Commands.DeletePost;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Application.Posts.Queries.GetPostById;
using Backtrack.Core.Application.Posts.Queries.GetPosts;
using Backtrack.Core.Application.Posts.Queries.GetSimilarPosts;
using Backtrack.Core.Application.Posts.Queries.SearchPostsBySemantic;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.WebApi.Contracts.Posts.Requests;
using Backtrack.Core.WebApi.Contracts.Posts.Responses;

namespace Backtrack.Core.WebApi.Mappings
{
    public static class PostMappings
    {
        // ==================== Request to Command/Query ====================

        public static CreatePostCommand ToCommand(this CreatePostRequest request, string authorId)
        {
            var postType = ParsePostType(request.PostType);

            return new CreatePostCommand
            {
                AuthorId = authorId,
                PostType = postType,
                ItemName = request.ItemName,
                Description = request.Description,
                DistinctiveMarks = request.DistinctiveMarks,
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

            return new GetPostsQuery
            {
                PagedQuery = PagedQuery.FromPage(request.Page, request.PageSize),
                PostType = postType,
                AuthorId = request.AuthorId,
                SearchTerm = request.SearchTerm,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RadiusInKm = request.RadiusInKm
            };
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

        public static GetSimilarPostsQuery ToQuery(this GetSimilarPostsRequest request)
        {
            return new GetSimilarPostsQuery
            {
                PostId = request.PostId,
                Limit = request.Limit
            };
        }

        public static GetPostByIdQuery ToQuery(Guid postId)
        {
            return new GetPostByIdQuery
            {
                PostId = postId
            };
        }

        public static DeletePostCommand ToCommand(Guid postId)
        {
            return new DeletePostCommand
            {
                PostId = postId
            };
        }

        // ==================== Result to Response ====================

        public static PostResponse ToResponse(this PostResult result)
        {
            return new PostResponse
            {
                Id = result.Id,
                AuthorId = result.AuthorId,
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

        public static GetSimilarPostsResponse ToResponse(this GetSimilarPostsResult result)
        {
            return new GetSimilarPostsResponse
            {
                EmbeddingStatus = result.EmbeddingStatus,
                IsReady = result.IsReady,
                SimilarPosts = result.SimilarPosts.Select(item => new SimilarPostResponse
                {
                    Id = item.Id,
                    PostType = item.PostType,
                    ItemName = item.ItemName,
                    Description = item.Description,
                    ImageUrls = item.ImageUrls,
                    Location = item.Location?.ToResponse(),
                    ExternalPlaceId = item.ExternalPlaceId,
                    DisplayAddress = item.DisplayAddress,
                    EventTime = item.EventTime,
                    CreatedAt = item.CreatedAt,
                    SimilarityScore = item.SimilarityScore
                })
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
