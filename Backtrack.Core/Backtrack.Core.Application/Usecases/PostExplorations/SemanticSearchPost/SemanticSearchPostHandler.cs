using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
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
            var result = await postRepository.GetPagedAsync(
                PagedQuery.Default,
                new PostFilters { Status = Domain.Constants.PostStatus.Active },
                cancellationToken);
            return result.Items.Select(p => new SearchPostResult
            {
                Id = p.Id,
                Author = p.Author?.ToPostAuthorResult(),
                Organization = p.Organization?.ToOrganizationOnPost(),
                PostType = p.PostType,
                Item = p.Item,
                ImageUrls = p.ImageUrls,
                Location = p.Location,
                ExternalPlaceId = p.ExternalPlaceId,
                DisplayAddress = p.DisplayAddress,
                EventTime = p.EventTime,
                CreatedAt = p.CreatedAt,
                Score = 0,
                DistanceInMeters = null
            });
        }

        var embedding  = await embeddingService.GenerateQueryEmbeddingAsync(command.Query, cancellationToken);
        var candidates = await postRepository.SearchBySemanticAsync(embedding, command.Filters, cancellationToken);

        var searchLocation = command.Filters?.Geo?.Location;

        return candidates
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
                Score            = x.SimilarityScore,
                DistanceInMeters = searchLocation != null && x.Post.Location != null
                    ? GeoUtil.Haversine(searchLocation, x.Post.Location)
                    : null
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.DistanceInMeters ?? double.MaxValue)
            .ToList();
    }

}
