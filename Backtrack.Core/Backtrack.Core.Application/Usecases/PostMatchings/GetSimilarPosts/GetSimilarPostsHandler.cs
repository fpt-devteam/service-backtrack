using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed class GetSimilarPostsHandler : IRequestHandler<GetSimilarPostsQuery, GetSimilarPostsResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMatchRepository _postMatchRepository;

    public GetSimilarPostsHandler(IPostRepository postRepository, IPostMatchRepository postMatchRepository)
    {
        _postRepository = postRepository;
        _postMatchRepository = postMatchRepository;
    }

    public async Task<GetSimilarPostsResult> Handle(GetSimilarPostsQuery request, CancellationToken cancellationToken)
    {
        var sourcePost = await _postRepository.GetByIdAsync(request.PostId);
        if (sourcePost == null)
        {
            throw new NotFoundException(PostErrors.NotFound);
        }

        var matches = await _postMatchRepository.GetMatchesByPostIdAsync(request.PostId, cancellationToken);

        var results = matches.Select(match =>
        {
            var targetPost = sourcePost.PostType == PostType.Lost ? match.FoundPost : match.LostPost;

            return new SimilarPostItem
            {
                Id = targetPost.Id,
                PostType = targetPost.PostType.ToString(),
                ItemName = targetPost.ItemName,
                Description = targetPost.Description,
                ImageUrls = targetPost.ImageUrls,
                Location = targetPost.Location,
                ExternalPlaceId = targetPost.ExternalPlaceId,
                DisplayAddress = targetPost.DisplayAddress,
                EventTime = targetPost.EventTime,
                MatchScore = match.MatchScore,
                DistanceMeters = match.DistanceMeters
            };
        }).ToList();

        return new GetSimilarPostsResult { SimilarPosts = results };
    }
}
