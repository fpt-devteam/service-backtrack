using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostMatchings.FindAndSavePostMatches;

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

        if (sourcePost.ContentEmbeddingStatus != ContentEmbeddingStatus.Ready || sourcePost.MultimodalEmbedding == null)
        {
            return Unit.Value;
        }

        sourcePost.PostMatchingStatus = PostMatchingStatus.Processing;
        _postRepository.Update(sourcePost);
        await _postRepository.SaveChangesAsync();

        try
        {
            if (sourcePost.PostType == PostType.Lost)
            {
                await _postMatchRepository.DeleteByLostPostIdsAsync(new[] { sourcePost.Id }, cancellationToken);
            }
            else
            {
                await _postMatchRepository.DeleteByFoundPostIdsAsync(new[] { sourcePost.Id }, cancellationToken);
            }
            await _postMatchRepository.SaveChangesAsync();

            var similarPosts = await _postRepository.GetSimilarPostsAsync(sourcePost, cancellationToken);

            var postMatches = similarPosts.Select(item =>
            {
                var (post, similarity, distanceMeters) = item;

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
                    MatchScore = (float)similarity,
                    DistanceMeters = (float)distanceMeters,
                    CreatedAt = DateTimeOffset.UtcNow
                };
            }).ToList();

            if (postMatches.Any())
            {
                await _postMatchRepository.CreateRangeAsync(postMatches, cancellationToken);
                await _postMatchRepository.SaveChangesAsync();
            }

            sourcePost.PostMatchingStatus = PostMatchingStatus.Completed;
            _postRepository.Update(sourcePost);
            await _postRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            sourcePost.PostMatchingStatus = PostMatchingStatus.Failed;
            _postRepository.Update(sourcePost);
            await _postRepository.SaveChangesAsync();

            throw; // Re-throw to allow Hangfire to retry
        }

        return Unit.Value;
    }
}
