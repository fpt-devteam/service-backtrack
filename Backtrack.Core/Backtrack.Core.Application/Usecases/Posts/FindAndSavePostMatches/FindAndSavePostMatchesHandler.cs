using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils.PostSimilarity;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.FindAndSavePostMatches;

public sealed class FindAndSavePostMatchesHandler : IRequestHandler<FindAndSavePostMatchesCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMatchRepository _postMatchRepository;

    public FindAndSavePostMatchesHandler(
        IPostRepository postRepository,
        IPostMatchRepository postMatchRepository)
    {
        _postRepository = postRepository;
        _postMatchRepository = postMatchRepository;
    }

    public async Task<Unit> Handle(FindAndSavePostMatchesCommand request, CancellationToken cancellationToken)
    {
        var sourcePost = await _postRepository.GetByIdAsync(request.PostId, true);
        if (sourcePost == null)
        {
            throw new NotFoundException(PostErrors.NotFound);
        }

        if (sourcePost.ContentEmbeddingStatus != ContentEmbeddingStatus.Ready || sourcePost.ContentEmbedding == null)
        {
            return Unit.Value; // Embedding not ready, cannot find matches yet
        }

        // 1. Delete old match records involving this post
        if (sourcePost.PostType == PostType.Lost)
        {
            await _postMatchRepository.DeleteByLostPostIdsAsync(new[] { sourcePost.Id }, cancellationToken);
        }
        else
        {
            await _postMatchRepository.DeleteByFoundPostIdsAsync(new[] { sourcePost.Id }, cancellationToken);
        }
        await _postMatchRepository.SaveChangesAsync();

        // 2. Find new potential matches
        var similarPosts = await _postRepository.GetSimilarPostsAsync(sourcePost, cancellationToken);

        // 3. Filter and map to PostMatch entities
        var postMatches = similarPosts
            .Where(item => item.SimilarityScore.TotalSimilarity >= SimilarityCriteria.TotalSimilarityThreshold)
            .Select(item =>
            {
                var (post, score) = item;

                Guid lostPostId, foundPostId;
                if (sourcePost.PostType == PostType.Lost)
                {
                    lostPostId = sourcePost.Id;
                    foundPostId = post.Id;
                }
                else
                {
                    lostPostId = post.Id;
                    foundPostId = sourcePost.Id;
                }

                return new PostMatch
                {
                    Id = Guid.NewGuid(),
                    LostPostId = lostPostId,
                    FoundPostId = foundPostId,
                    MatchScore = (float)score.TotalSimilarity,
                    LocationScore = (float)score.LocationSimilarity,
                    DescriptionScore = (float)score.DescriptionSimilarity,
                    DistanceMeters = (float)score.DistanceMeters,
                    CreatedAt = DateTimeOffset.UtcNow
                };
            })
            .ToList();

        // 4. Save new matches
        if (postMatches.Any())
        {
            await _postMatchRepository.CreateRangeAsync(postMatches, cancellationToken);
            await _postMatchRepository.SaveChangesAsync();
        }

        return Unit.Value;
    }
}
