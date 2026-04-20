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

        if (sourcePost.EmbeddingStatus != EmbeddingStatus.Ready || sourcePost.Embedding is null)
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
            // ── 4. Delete stale matches ───────────────────────────────────────
            await postMatchRepository.DeleteBySourcePostIdsAsync([sourcePost.Id], cancellationToken);
            await postMatchRepository.SaveChangesAsync();

            // ── 5. Find similar posts ─────────────────────────────────────────
            var similarPosts = await postRepository.GetSimilarPostsAsync(sourcePost, cancellationToken);
            var postMatches  = new List<PostMatch>();

            foreach (var (candidatePost, similarity) in similarPosts)
            {
                var (lostPost, foundPost) = sourcePost.PostType == PostType.Lost
                    ? (sourcePost, candidatePost)
                    : (candidatePost, sourcePost);

                var context = new PostMatchContext
                {
                    Category        = sourcePost.Category,
                    LostDescription = BuildAssessorDescription(lostPost),
                    FoundDescription = BuildAssessorDescription(foundPost),
                    DistanceMeters  = (float)GeoUtil.Haversine(sourcePost.Location, candidatePost.Location),
                    TimeGapDays     = Math.Abs((sourcePost.EventTime - candidatePost.EventTime).TotalDays),
                    MatchScore      = (float)similarity,
                    MatchingLevel   = ScoreToMatchingLevel((float)similarity),
                };
                logger.LogInformation(
                    "Assessing match between Post {SourcePostId} and Candidate {CandidatePostId} with similarity {Similarity:P0}.",
                    sourcePost.Id, candidatePost.Id, similarity);

                List<MatchEvidence> evidence;
                try
                {
                    var assessment = await assessor.AssessAsync(context, cancellationToken);
                    evidence = assessment.Evidence;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Assessor failed for candidate {CandidateId} — saving match without evidence.", candidatePost.Id);
                    evidence = [];
                }

                postMatches.Add(new PostMatch
                {
                    Id              = Guid.NewGuid(),
                    SourcePostId    = sourcePost.Id,
                    CandidatePostId = candidatePost.Id,
                    Score           = similarity,
                    Evidence        = evidence,
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    // Cards are excluded from PostDocumentUtil (privacy); built separately here.
    private static string BuildAssessorDescription(Post post)
    {
        if (post.Category == ItemCategory.Cards)
            return post.CardDetail is { } c ? BuildCardDescription(c) : "Card (no details available).";

        return PostDocumentUtil.BuildDocument(post);
    }

    private static string BuildCardDescription(PostCardDetail c)
    {
        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrWhiteSpace(c.ItemName))           sb.Append($"Card: {c.ItemName}. ");
        if (!string.IsNullOrWhiteSpace(c.HolderName))         sb.Append($"Holder name: {c.HolderName}. ");
        if (!string.IsNullOrWhiteSpace(c.IssuingAuthority))   sb.Append($"Issuing authority: {c.IssuingAuthority}. ");
        if (c.ExpiryDate.HasValue)                             sb.Append($"Expiry date: {c.ExpiryDate:MM/yyyy}. ");
        if (c.DateOfBirth.HasValue)                            sb.Append($"Date of birth: {c.DateOfBirth:dd/MM/yyyy}. ");
        if (!string.IsNullOrWhiteSpace(c.OcrText))             sb.Append($"OCR text: {c.OcrText}. ");
        if (!string.IsNullOrWhiteSpace(c.AdditionalDetails))   sb.Append($"Notes: {c.AdditionalDetails}.");
        return sb.Length > 0 ? sb.ToString().Trim() : "Card (no details available).";
    }

    private static MatchingLevel ScoreToMatchingLevel(float score) => score switch
    {
        >= 0.85f => MatchingLevel.VeryHigh,
        >= 0.65f => MatchingLevel.High,
        _        => MatchingLevel.Medium,
    };
}
