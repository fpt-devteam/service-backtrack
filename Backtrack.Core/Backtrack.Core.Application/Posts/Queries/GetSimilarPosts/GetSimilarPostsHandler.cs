using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.GetSimilarPosts;

public sealed class GetSimilarPostsHandler : IRequestHandler<GetSimilarPostsQuery, GetSimilarPostsResult>
{
    private readonly IPostRepository _postRepository;
    private const double RadiusInKm = 20.0; // Fixed 20km radius

    public GetSimilarPostsHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<GetSimilarPostsResult> Handle(GetSimilarPostsQuery request, CancellationToken cancellationToken)
    {
        var sourcePost = await _postRepository.GetByIdAsync(request.PostId);

        if (sourcePost == null)
        {
            throw new NotFoundException(PostErrors.NotFound);
        }

        if (sourcePost.ContentEmbeddingStatus != ContentEmbeddingStatus.Ready)
        {
            return new GetSimilarPostsResult
            {
                EmbeddingStatus = sourcePost.ContentEmbeddingStatus.ToString(),
                IsReady = false,
                SimilarPosts = Array.Empty<SimilarPostItem>()
            };
        }

        if (sourcePost.ContentEmbedding == null || sourcePost.ContentEmbedding.Length == 0)
        {
            throw new InvalidOperationException($"Post {request.PostId} has Ready status but missing embedding");
        }

        var similarPosts = await _postRepository.GetSimilarPostsAsync(
            postId: request.PostId,
            postType: sourcePost.PostType,
            embedding: sourcePost.ContentEmbedding,
            latitude: sourcePost.Location?.Latitude,
            longitude: sourcePost.Location?.Longitude,
            radiusInKm: RadiusInKm,
            limit: request.Limit,
            cancellationToken: cancellationToken);

        var results = similarPosts.Select(item =>
        {
            var (post, similarityScore) = item;
            return new SimilarPostItem
            {
                Id = post.Id,
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
                CreatedAt = post.CreatedAt,
                SimilarityScore = similarityScore
            };
        }).ToList();

        return new GetSimilarPostsResult
        {
            EmbeddingStatus = ContentEmbeddingStatus.Ready.ToString(),
            IsReady = true,
            SimilarPosts = results
        };
    }
}
