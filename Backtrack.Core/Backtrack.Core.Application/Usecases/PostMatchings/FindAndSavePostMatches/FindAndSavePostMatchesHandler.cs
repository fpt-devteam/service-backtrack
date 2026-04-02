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
    ILlmService llmService,
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

        // ── 2. Hash check — skip if matching already completed for current content ──
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

            var sourceContext = BuildPostContext(sourcePost);
            var postMatches = new List<PostMatch>();

            foreach (var (candidatePost, similarity, distanceMeters) in similarPosts)
            {
                // ── 6. Compute per-criteria scores ────────────────────────────
                var matchScore       = PostMatchingCriteria.ComputeWeightedScore(similarity, distanceMeters);
                var descriptionScore = PostMatchingCriteria.ComputeDescriptionScore(similarity);
                var visualScore      = PostMatchingCriteria.ComputeVisualScore(similarity);
                var locationScore    = PostMatchingCriteria.ComputeLocationScore(distanceMeters);

                // Determine lost/found roles and build candidate context
                Guid lostPostId, foundPostId;
                PostMatchContext lostContext, foundContext;

                if (sourcePost.PostType == PostType.Lost)
                {
                    lostPostId   = sourcePost.Id;
                    foundPostId  = candidatePost.Id;
                    lostContext  = sourceContext;
                    foundContext = BuildPostContext(candidatePost);
                }
                else
                {
                    lostPostId   = candidatePost.Id;
                    foundPostId  = sourcePost.Id;
                    lostContext  = BuildPostContext(candidatePost);
                    foundContext = sourceContext;
                }

                var timeWindowScore = PostMatchingCriteria.ComputeTimeWindowScore(
                    lostContext.EventTime,
                    foundContext.EventTime);

                // ── 7. LLM assessment per match ───────────────────────────────
                PostMatchAssessment? assessment = null;
                try
                {
                    assessment = await llmService.AssessPostMatchAsync(
                        lostContext,
                        foundContext,
                        new PostMatchScores
                        {
                            DescriptionScore = descriptionScore,
                            VisualScore      = visualScore,
                            LocationScore    = locationScore,
                            TimeWindowScore  = timeWindowScore
                        },
                        (float)distanceMeters,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "LLM assessment failed for LostPost {LostPostId} / FoundPost {FoundPostId}. Saving without assessment.",
                        lostPostId, foundPostId);
                }

                postMatches.Add(new PostMatch
                {
                    Id                 = Guid.NewGuid(),
                    LostPostId         = lostPostId,
                    FoundPostId        = foundPostId,
                    MatchScore         = matchScore,
                    MatchingLevel      = PostMatchingCriteria.ComputeMatchingLevel(matchScore),
                    DistanceMeters     = (float)distanceMeters,
                    DescriptionScore   = descriptionScore,
                    VisualScore        = visualScore,
                    LocationScore      = locationScore,
                    TimeWindowScore    = timeWindowScore,
                    IsAssessed         = assessment is not null,
                    AssessmentSummary  = assessment?.Summary,
                    CriteriaAssessment = assessment?.Criteria,
                    CreatedAt          = DateTimeOffset.UtcNow
                });
            }

            // ── 8. Persist matches ────────────────────────────────────────────
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

            throw; // Re-throw to allow Hangfire to retry
        }

        return Unit.Value;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PostMatchContext BuildPostContext(Post post)
    {
        return new PostMatchContext
        {
            ItemName      = post.Item.ItemName,
            Description   = post.Item.AdditionalDetails,
            EventTime     = post.EventTime,
            DisplayAddress = post.DisplayAddress,
            ImageUrl      = post.ImageUrls.FirstOrDefault()
        };
    }
}
