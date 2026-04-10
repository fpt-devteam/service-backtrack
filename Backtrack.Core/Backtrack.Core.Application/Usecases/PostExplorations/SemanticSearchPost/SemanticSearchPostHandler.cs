using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SemanticSearchPost;

public sealed class SemanticSearchPostHandler(
    IPostRepository postRepository,
    IEmbeddingService embeddingService,
    ICrossEncoderService crossEncoder)
    : IRequestHandler<SemanticSearchPostCommand, IEnumerable<SearchPostResult>>
{
    /// <summary>Max candidates pulled from vector search before reranking.</summary>
    private const int CandidateLimit = 20;

    /// <summary>Minimum rerank score to include in final results.</summary>
    private const double RerankThreshold = 0.1;

    public async Task<IEnumerable<SearchPostResult>> Handle(
        SemanticSearchPostCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Retrieve candidates via embedding similarity
        var embedding  = await embeddingService.GenerateQueryEmbeddingAsync(command.Query, cancellationToken);
        var candidates = (await postRepository.SearchBySemanticAsync(embedding, command.Filters, cancellationToken))
            .Take(CandidateLimit)
            .ToList();

        if (candidates.Count == 0)
            return [];

        // 2. Build flat text documents for cross-encoder scoring
        var documents = candidates.Select(c => PostDocumentUtil.BuildDocument(c.Post)).ToList();

        // 3. Cross-encoder reranking — single Qwen3-Reranker call for the whole batch
        // var scores = await crossEncoder.ScoreAsync(command.Query, documents, cancellationToken);

        // 4. Merge scores and sort by cross-encoder relevance descending
        var searchLocation = command.Filters?.Geo?.Location;

        return candidates
            .Select((c, i) => (Post: c.Post, SemanticScore: c.SimilarityScore, RerankScore: 1))
            // .Where(x => x.RerankScore >= RerankThreshold)
            .OrderByDescending(x => x.RerankScore)
            .Select(x => new SearchPostResult
            {
                Id               = x.Post.Id,
                Author           = x.Post.Author?.ToPostAuthorResult(),
                Organization     = x.Post.Organization?.ToOrganizationOnPost(),
                PostType         = x.Post.PostType,
                Item             = x.Post.Item,
                ImageUrls        = x.Post.ImageUrls,
                Location         = x.Post.Location,
                ExternalPlaceId  = x.Post.ExternalPlaceId,
                DisplayAddress   = x.Post.DisplayAddress,
                EventTime        = x.Post.EventTime,
                CreatedAt        = x.Post.CreatedAt,
                Score            = x.SemanticScore,
                DistanceInMeters = searchLocation != null && x.Post.Location != null
                    ? GeoUtil.Haversine(searchLocation, x.Post.Location)
                    : null
            });
    }

}
