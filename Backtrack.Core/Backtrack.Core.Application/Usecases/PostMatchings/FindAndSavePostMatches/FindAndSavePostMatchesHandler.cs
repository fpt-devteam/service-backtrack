using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.FindAndSavePostMatches;

public sealed class FindAndSavePostMatchesHandler(
    IPostRepository postRepository,
    IPostMatchRepository postMatchRepository,
    IPostMatchAssessor assessor,
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
            // ── 4. Delete all existing matches for this post ──────────────────
            if (sourcePost.PostType == PostType.Lost)
                await postMatchRepository.DeleteByLostPostIdsAsync([sourcePost.Id], cancellationToken);
            else
                await postMatchRepository.DeleteByFoundPostIdsAsync([sourcePost.Id], cancellationToken);
            await postMatchRepository.SaveChangesAsync();

            // ── 5. Find similar posts ─────────────────────────────────────────
            var similarPosts = await postRepository.GetSimilarPostsAsync(sourcePost, cancellationToken);
            var postMatches  = new List<PostMatch>();

            foreach (var (candidatePost, similarity) in similarPosts)
            {
                var distanceMeters = GeoUtil.Haversine(sourcePost.Location, candidatePost.Location);
                var matchScore     = (float)similarity;
                var matchingLevel  = PostMatchingUtils.ComputeMatchingLevel(matchScore);

                // Determine lost/found roles
                Guid lostPostId, foundPostId;
                Post lostPost, foundPost;

                if (sourcePost.PostType == PostType.Lost)
                {
                    lostPostId  = sourcePost.Id;   lostPost  = sourcePost;
                    foundPostId = candidatePost.Id; foundPost = candidatePost;
                }
                else
                {
                    lostPostId  = candidatePost.Id; lostPost  = candidatePost;
                    foundPostId = sourcePost.Id;    foundPost = sourcePost;
                }

                var timeGapDays = Math.Abs((lostPost.EventTime - foundPost.EventTime).TotalDays);

                // ── 6. LLM assessment ─────────────────────────────────────────
                var assessmentSummary = string.Empty;
                var isAssessed        = false;
                try
                {
                    var assessment = await assessor.AssessAsync(new PostMatchContext
                    {
                        LostDescription  = PostDocumentUtil.BuildDocument(lostPost),
                        FoundDescription = PostDocumentUtil.BuildDocument(foundPost),
                        DistanceMeters   = (float)distanceMeters,
                        TimeGapDays      = timeGapDays,
                        MatchScore       = matchScore,
                        MatchingLevel    = matchingLevel
                    }, cancellationToken);

                    assessmentSummary = assessment.Summary;
                    isAssessed        = true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "LLM assessment failed for LostPost {LostPostId} / FoundPost {FoundPostId}. Saving without assessment.",
                        lostPostId, foundPostId);
                }

                postMatches.Add(new PostMatch
                {
                    Id                = Guid.NewGuid(),
                    LostPostId        = lostPostId,
                    FoundPostId       = foundPostId,
                    MatchScore        = matchScore,
                    MatchingLevel     = matchingLevel,
                    DistanceMeters    = (float)distanceMeters,
                    TimeGapDays       = timeGapDays,
                    IsAssessed        = isAssessed,
                    AssessmentSummary = assessmentSummary,
                    CreatedAt         = DateTimeOffset.UtcNow
                });
            }

            // ── 7. Persist matches ────────────────────────────────────────────
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
