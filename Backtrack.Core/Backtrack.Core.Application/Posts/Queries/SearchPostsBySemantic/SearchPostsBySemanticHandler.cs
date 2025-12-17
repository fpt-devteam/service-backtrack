using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.SearchPostsBySemantic;

public sealed class SearchPostsBySemanticHandler : IRequestHandler<SearchPostsBySemanticQuery, PagedResult<PostSemanticSearchResult>>
{
    private readonly IPostRepository _postRepository;
    private readonly IEmbeddingService _embeddingService;

    public SearchPostsBySemanticHandler(IPostRepository postRepository, IEmbeddingService embeddingService)
    {
        _postRepository = postRepository;
        _embeddingService = embeddingService;
    }

    public async Task<PagedResult<PostSemanticSearchResult>> Handle(SearchPostsBySemanticQuery query, CancellationToken cancellationToken)
    {
        // Generate embedding for search text with enhanced context
        // Format query similarly to post embeddings for better matching
        var enhancedQuery = $@"I am searching for: {query.SearchText}
Item description: {query.SearchText}

Looking for information about {query.SearchText.ToLower()}.";

        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(enhancedQuery, cancellationToken);

        // Search posts by semantic similarity (only searches posts with status = Ready)
        var (items, totalCount) = await _postRepository.SearchBySemanticAsync(
            queryEmbedding: queryEmbedding,
            offset: query.PagedQuery.Offset,
            limit: query.PagedQuery.Limit,
            postType: query.PostType,
            latitude: query.Latitude,
            longitude: query.Longitude,
            radiusInKm: query.RadiusInKm,
            cancellationToken: cancellationToken);

        // Map to result DTOs
        var results = items.Select(item =>
        {
            var (post, similarityScore) = item;
            return new PostSemanticSearchResult
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

        return new PagedResult<PostSemanticSearchResult>(totalCount, results);
    }
}
