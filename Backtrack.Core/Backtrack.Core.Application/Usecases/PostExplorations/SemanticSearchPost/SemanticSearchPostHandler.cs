using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SemanticSearchPost;

public sealed class SemanticSearchPostHandler(
    IPostRepository postRepository,
    IEmbeddingService embeddingService)
    : IRequestHandler<SemanticSearchPostCommand, IEnumerable<SearchPostResult>>
{
    public async Task<IEnumerable<SearchPostResult>> Handle(
        SemanticSearchPostCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Query))
        {
            var filter = command.Filters ?? new PostFilters { Status = PostStatus.Active };

            var result = await postRepository.GetPagedAsync(PagedQuery.Default, filter, cancellationToken);
            return result.Items.Select(p => p.ToSearchPostResult(score: 0));
        }

        var embedding  = await embeddingService.GenerateQueryEmbeddingAsync(command.Query, cancellationToken);
        var candidates = await postRepository.SearchBySemanticAsync(embedding, command.Filters, cancellationToken);

        var searchLocation = command.Filters?.Geo?.Location;

        return candidates
            .Select(x => x.Post.ToSearchPostResult(
                score: x.SimilarityScore,
                distanceInMeters: searchLocation != null && x.Post.Location != null
                    ? GeoUtil.Haversine(searchLocation, x.Post.Location)
                    : null))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.DistanceInMeters ?? double.MaxValue)
            .ToList();
    }
}
