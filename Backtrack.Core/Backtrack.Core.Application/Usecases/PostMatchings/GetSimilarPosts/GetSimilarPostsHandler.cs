using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed class GetSimilarPostsHandler : IRequestHandler<GetSimilarPostsQuery, GetSimilarPostsResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMatchRepository _postMatchRepository;
    private readonly IPostMatchAssessor _assessor;
    private readonly ILogger<GetSimilarPostsHandler> _logger;

    public GetSimilarPostsHandler(
        IPostRepository postRepository,
        IPostMatchRepository postMatchRepository,
        IPostMatchAssessor assessor,
        ILogger<GetSimilarPostsHandler> logger)
    {
        _postRepository      = postRepository;
        _postMatchRepository = postMatchRepository;
        _assessor            = assessor;
        _logger              = logger;
    }

    public async Task<GetSimilarPostsResult> Handle(GetSimilarPostsQuery request, CancellationToken cancellationToken)
    {
        var sourcePost = await _postRepository.GetByIdAsync(request.PostId)
            ?? throw new NotFoundException(PostErrors.NotFound);

        var matches = (await _postMatchRepository.GetMatchesByPostIdAsync(request.PostId, cancellationToken))
            .Take(request.Limit)
            .ToList();

        // Run LLM assessment for any match that wasn't assessed during background job
        var unassessed = matches.Where(m => !m.IsAssessed).ToList();
        if (unassessed.Count > 0)
        {
            foreach (var match in unassessed)
                await TryRunAssessmentAsync(match, sourcePost, cancellationToken);

            await _postMatchRepository.SaveChangesAsync();
        }

        var results = matches.Select(match => ToSimilarPostItem(match, sourcePost)).ToList();
        return new GetSimilarPostsResult { SimilarPosts = results };
    }

    private async Task TryRunAssessmentAsync(PostMatch match, Post sourcePost, CancellationToken cancellationToken)
    {
        try
        {
            var targetPost = sourcePost.PostType == PostType.Lost ? match.FoundPost : match.LostPost;

            var (lostPost, foundPost) = sourcePost.PostType == PostType.Lost
                ? (sourcePost, targetPost)
                : (targetPost, sourcePost);

            var timeGapDays = Math.Abs((lostPost.EventTime - foundPost.EventTime).TotalDays);

            var assessment = await _assessor.AssessAsync(new PostMatchContext
            {
                LostDescription  = PostDocumentUtil.BuildDocument(lostPost),
                FoundDescription = PostDocumentUtil.BuildDocument(foundPost),
                DistanceMeters   = match.DistanceMeters,
                TimeGapDays      = timeGapDays,
                MatchScore       = match.MatchScore,
                MatchingLevel    = match.MatchingLevel
            }, cancellationToken);

            match.IsAssessed        = true;
            match.AssessmentSummary = assessment.Summary;
            match.UpdatedAt         = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lazy LLM assessment failed for PostMatch {MatchId}.", match.Id);
        }
    }

    private static SimilarPostItem ToSimilarPostItem(PostMatch match, Post sourcePost)
    {
        var targetPost = sourcePost.PostType == PostType.Lost ? match.FoundPost : match.LostPost;

        return new SimilarPostItem
        {
            Id                = targetPost.Id,
            PostType          = targetPost.PostType,
            Item              = targetPost.Item,
            ImageUrls         = targetPost.ImageUrls,
            Location          = targetPost.Location,
            ExternalPlaceId   = targetPost.ExternalPlaceId,
            DisplayAddress    = targetPost.DisplayAddress,
            EventTime         = targetPost.EventTime,
            MatchScore        = match.MatchScore,
            DistanceMeters    = match.DistanceMeters,
            TimeGapDays       = match.TimeGapDays,
            MatchingLevel     = match.MatchingLevel,
            IsAssessed        = match.IsAssessed,
            AssessmentSummary = match.AssessmentSummary
        };
    }
}
