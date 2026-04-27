using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Application.Usecases.Notifications.SendPushNotification;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.FindAndSavePostMatches;

public sealed class FindAndSavePostMatchesHandler(
    IPostRepository postRepository,
    IPostMatchRepository postMatchRepository,
    IPostMatchAssessor assessor,
    IMediator mediator,
    ILogger<FindAndSavePostMatchesHandler> logger) : IRequestHandler<FindAndSavePostMatchesCommand>
{
    public async Task<Unit> Handle(FindAndSavePostMatchesCommand request, CancellationToken cancellationToken)
    {
        var sourcePost = await postRepository.GetByIdAsync(request.PostId, isTrack: true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (sourcePost.PostMatchingStatus == PostMatchingStatus.Completed)
        {
            logger.LogDebug("Post {PostId} matching already completed, skipping.", sourcePost.Id);
            return Unit.Value;
        }

        await BeginProcessingAsync(sourcePost);

        try
        {
            await postMatchRepository.DeleteByPostIdAsync(sourcePost.Id, cancellationToken);
            await postMatchRepository.SaveChangesAsync();

            var allMatchPairs = new List<(PostMatch Match, Post Candidate)>();

            // Step 1: Card matching — instant, no AI.
            // Only runs for Cards category. Finds opposite-type posts in the same subcategory within the distance and time window.
            if (sourcePost.Category == ItemCategory.Cards)
            {
                var cardMatchPairs = await RunCardMatchingAsync(sourcePost, cancellationToken);
                allMatchPairs.AddRange(cardMatchPairs);
                logger.LogInformation("Card matching: {Count} match(es) for Post {PostId}.", cardMatchPairs.Count, sourcePost.Id);
            }

            // Step 2: AI similarity — only runs when embeddings are ready.
            // Skips candidates already matched in step 1.
            if (sourcePost.EmbeddingStatus == EmbeddingStatus.Ready && sourcePost.Embedding is not null)
            {
                var alreadyMatchedIds = GetCandidateIds(sourcePost.Id, allMatchPairs.Select(p => p.Match).ToList());
                var aiMatchPairs = await RunAiMatchingAsync(sourcePost, alreadyMatchedIds, cancellationToken);
                allMatchPairs.AddRange(aiMatchPairs);
                logger.LogInformation("AI matching: {Count} match(es) for Post {PostId}.", aiMatchPairs.Count, sourcePost.Id);
            }
            else
            {
                logger.LogWarning("Post {PostId} embeddings not ready — skipping AI matching.", sourcePost.Id);
            }

            var allMatches = allMatchPairs.Select(p => p.Match).ToList();
            if (allMatches.Count > 0)
            {
                await postMatchRepository.CreateRangeAsync(allMatches, cancellationToken);
                await postMatchRepository.SaveChangesAsync();
            }

            await CompleteProcessingAsync(sourcePost, allMatches.Count);

            var readyPairs = allMatchPairs.Where(p => p.Match.Status == MatchStatus.ReadyToShow).ToList();
            await SendMatchNotificationsAsync(sourcePost, readyPairs, cancellationToken);
        }
        catch (Exception ex)
        {
            await FailProcessingAsync(sourcePost, ex);
            throw;
        }

        return Unit.Value;
    }

    // ── Card matching ─────────────────────────────────────────────────────────

    private async Task<List<(PostMatch, Post)>> RunCardMatchingAsync(Post sourcePost, CancellationToken ct)
    {
        var candidates = await postRepository.GetCardMatchCandidatesAsync(sourcePost, ct);
        return candidates.Select(candidate => (BuildCardMatch(sourcePost, candidate), candidate)).ToList();
    }

    private static PostMatch BuildCardMatch(Post sourcePost, Post candidate)
    {
        var (lostPost, foundPost) = sourcePost.PostType == PostType.Lost
            ? (sourcePost, candidate)
            : (candidate, sourcePost);

        var distanceMeters = GeoUtil.Haversine(sourcePost.Location, candidate.Location);
        var timeGapDays    = Math.Abs((sourcePost.EventTime - candidate.EventTime).TotalDays);
        var evidence       = BuildCardEvidence(lostPost, foundPost, distanceMeters, timeGapDays);

        return new PostMatch
        {
            Id             = Guid.NewGuid(),
            LostPostId     = lostPost.Id,
            FoundPostId    = foundPost.Id,
            Score          = PostSimilarityThresholds.VerySimilarityHighThreshold,
            Status         = MatchStatus.ReadyToShow,
            DistanceMeters = distanceMeters,
            TimeGapDays    = timeGapDays,
            Reasoning      = "Matched by card number hash or holder name, confirmed by location and time window.",
            Evidence       = evidence,
            CreatedAt      = DateTimeOffset.UtcNow
        };
    }

    private static List<MatchEvidence> BuildCardEvidence(Post lostPost, Post foundPost, double distanceMeters, double timeGapDays)
    {
        var evidence = new List<MatchEvidence>();

        var lostCard  = lostPost.CardDetail;
        var foundCard = foundPost.CardDetail;

        if (lostCard?.CardNumberHash is not null && foundCard?.CardNumberHash is not null)
            evidence.Add(new MatchEvidence("CardNumber", MatchStrength.Strong,
                lostCard.CardNumberMasked ?? "***",
                foundCard.CardNumberMasked ?? "***",
                "Card number hash matched exactly"));

        if (lostCard?.HolderNameNormalized is not null && foundCard?.HolderNameNormalized is not null)
            evidence.Add(new MatchEvidence("HolderName", MatchStrength.Strong,
                lostCard.HolderName ?? lostCard.HolderNameNormalized,
                foundCard.HolderName ?? foundCard.HolderNameNormalized,
                "Holder name matched"));

        evidence.Add(new MatchEvidence("Location", MatchStrength.Strong,
            $"{lostPost.Location.Latitude:F4}, {lostPost.Location.Longitude:F4}",
            $"{foundPost.Location.Latitude:F4}, {foundPost.Location.Longitude:F4}",
            $"{distanceMeters / 1000:F1}km apart — within {PostSimilarityThresholds.MaxDistanceMeters / 1000}km threshold"));

        evidence.Add(new MatchEvidence("EventTime", MatchStrength.Strong,
            lostPost.EventTime.ToString("yyyy-MM-dd"),
            foundPost.EventTime.ToString("yyyy-MM-dd"),
            $"{timeGapDays:F1} day(s) apart — within {PostSimilarityThresholds.TimeWindowDays}-day window"));

        return evidence;
    }

    // ── AI matching ───────────────────────────────────────────────────────────

    private async Task<List<(PostMatch, Post)>> RunAiMatchingAsync(Post sourcePost, HashSet<Guid> excludedIds, CancellationToken ct)
    {
        var similarPosts = await postRepository.GetSimilarPostsAsync(sourcePost, ct);

        var matches = new List<(PostMatch, Post)>();
        foreach (var (candidate, similarity) in similarPosts.Where(s => !excludedIds.Contains(s.Post.Id)))
        {
            var match = await AssessAndBuildAiMatchAsync(sourcePost, candidate, similarity, ct);
            if (match is not null)
                matches.Add((match, candidate));
        }
        return matches;
    }

    private async Task<PostMatch?> AssessAndBuildAiMatchAsync(Post sourcePost, Post candidate, double similarity, CancellationToken ct)
    {
        var (lostPost, foundPost) = sourcePost.PostType == PostType.Lost
            ? (sourcePost, candidate)
            : (candidate, sourcePost);

        var context = new PostMatchContext
        {
            Category         = sourcePost.Category,
            LostDescription  = PostDocumentUtil.BuildDocument(lostPost),
            FoundDescription = PostDocumentUtil.BuildDocument(foundPost),
        };

        logger.LogInformation(
            "Assessing Lost {LostId} vs Found {FoundId} — similarity {Similarity:P0}.",
            lostPost.Id, foundPost.Id, similarity);

        PostMatchAssessment assessment;
        try
        {
            assessment = await assessor.AssessAsync(context, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Assessor failed for candidate {CandidateId} — skipping.", candidate.Id);
            return null;
        }

        return new PostMatch
        {
            Id             = Guid.NewGuid(),
            LostPostId     = lostPost.Id,
            FoundPostId    = foundPost.Id,
            Score          = similarity,
            Evidence       = assessment.Evidence,
            Status         = assessment.IsMatch ? MatchStatus.ReadyToShow : MatchStatus.RejectedByAI,
            DistanceMeters = (float)GeoUtil.Haversine(sourcePost.Location, candidate.Location),
            TimeGapDays    = Math.Abs((sourcePost.EventTime - candidate.EventTime).TotalDays),
            Reasoning      = assessment.Reasoning,
            CreatedAt      = DateTimeOffset.UtcNow
        };
    }

    // ── Notifications ─────────────────────────────────────────────────────────

    private async Task SendMatchNotificationsAsync(Post sourcePost, List<(PostMatch Match, Post Candidate)> readyPairs, CancellationToken ct)
    {
        foreach (var (match, candidate) in readyPairs)
        {
            var (lostPost, foundPost) = match.LostPostId == sourcePost.Id
                ? (sourcePost, candidate)
                : (candidate, sourcePost);

            var data = new NotificationData { ScreenPath = $"/posts/{sourcePost.Id}/matches" };
            var source = new NotificationSource { Name = "PostMatching", EventId = match.Id.ToString() };

            await mediator.Send(new SendPushNotificationCommand
            {
                Target = new NotificationTarget { UserId = lostPost.AuthorId },
                Title  = $"Match found: {foundPost.PostTitle}",
                Body   = "Your lost item may have been found. Check the match now.",
                Type   = NotificationEvent.AIMatchingEvent,
                Data   = data with { ScreenPath = $"/posts/{lostPost.Id}/matches" },
                Source = source with { EventId = $"{match.Id}:lost" }
            }, ct);

            await mediator.Send(new SendPushNotificationCommand
            {
                Target = new NotificationTarget { UserId = foundPost.AuthorId },
                Title  = $"Match found: {lostPost.PostTitle}",
                Body   = "The item you found may belong to someone. Check the match now.",
                Type   = NotificationEvent.AIMatchingEvent,
                Data   = data with { ScreenPath = $"/posts/{foundPost.Id}/matches" },
                Source = source with { EventId = $"{match.Id}:found" }
            }, ct);
        }
    }

    // ── State transitions ─────────────────────────────────────────────────────

    private async Task BeginProcessingAsync(Post post)
    {
        post.PostMatchingStatus = PostMatchingStatus.Processing;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();
    }

    private async Task CompleteProcessingAsync(Post post, int totalMatches)
    {
        post.PostMatchingStatus = PostMatchingStatus.Completed;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();
        logger.LogInformation("Matching completed for Post {PostId}. {Count} total match(es) saved.", post.Id, totalMatches);
    }

    private async Task FailProcessingAsync(Post post, Exception ex)
    {
        logger.LogError(ex, "Matching failed for Post {PostId}.", post.Id);
        post.PostMatchingStatus = PostMatchingStatus.Failed;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private static HashSet<Guid> GetCandidateIds(Guid sourcePostId, List<PostMatch> matches)
        => matches
            .Select(m => m.LostPostId == sourcePostId ? m.FoundPostId : m.LostPostId)
            .ToHashSet();
}
