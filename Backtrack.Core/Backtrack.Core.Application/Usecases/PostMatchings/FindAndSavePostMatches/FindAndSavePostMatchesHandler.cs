using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.FindAndSavePostMatches;

public sealed class FindAndSavePostMatchesHandler(
    IPostRepository postRepository,
    IPostMatchRepository postMatchRepository,
    ILogger<FindAndSavePostMatchesHandler> logger) : IRequestHandler<FindAndSavePostMatchesCommand>
{
    public async Task<Unit> Handle(FindAndSavePostMatchesCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Load & validate ────────────────────────────────────────────────
        var sourcePost = await postRepository.GetByIdAsync(request.PostId, isTrack: true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (sourcePost.EmbeddingStatus != EmbeddingStatus.Ready
            || sourcePost.Embedding is null)
        {
            logger.LogWarning("Post {PostId} embeddings are not ready, skipping matching.", sourcePost.Id);
            return Unit.Value;
        }

        // ── 2. Skip if matching already completed ─────────────────────────────
        if (sourcePost.PostMatchingStatus == PostMatchingStatus.Completed)
        {
            logger.LogDebug("Post {PostId} matching already up-to-date, skipping.", sourcePost.Id);
            return Unit.Value;
        }

        // ── 3. Mark as processing ─────────────────────────────────────────────
        sourcePost.PostMatchingStatus = PostMatchingStatus.Processing;
        postRepository.Update(sourcePost);
        await postRepository.SaveChangesAsync();

        try
        {
            // ── 4. Delete all existing matches where this post is the source ──
            await postMatchRepository.DeleteBySourcePostIdsAsync([sourcePost.Id], cancellationToken);
            await postMatchRepository.SaveChangesAsync();

            // ── 5. Find similar posts and save matches ────────────────────────
            var similarPosts = await postRepository.GetSimilarPostsAsync(sourcePost, cancellationToken);
            var postMatches  = new List<PostMatch>();

            foreach (var (candidatePost, similarity) in similarPosts)
            {
                postMatches.Add(new PostMatch
                {
                    Id              = Guid.NewGuid(),
                    SourcePostId    = sourcePost.Id,
                    CandidatePostId = candidatePost.Id,
                    Score           = similarity,
                    MatchReason     = "rrf_embedding",
                    Status          = MatchStatus.Pending,
                    CreatedAt       = DateTimeOffset.UtcNow
                });
            }

            // ── 6. Persist matches ────────────────────────────────────────────
            if (postMatches.Count > 0)
            {
                await postMatchRepository.CreateRangeAsync(postMatches, cancellationToken);
                await postMatchRepository.SaveChangesAsync();
            }

            sourcePost.PostMatchingStatus = PostMatchingStatus.Completed;
            postRepository.Update(sourcePost);
            await postRepository.SaveChangesAsync();

            logger.LogInformation(
                "Matching completed for Post {PostId}. {Count} match(es) saved.",
                sourcePost.Id, postMatches.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Matching failed for Post {PostId}. Marking as Failed.", sourcePost.Id);

            sourcePost.PostMatchingStatus = PostMatchingStatus.Failed;
            postRepository.Update(sourcePost);
            await postRepository.SaveChangesAsync();

            throw;
        }

        return Unit.Value;
    }
}
