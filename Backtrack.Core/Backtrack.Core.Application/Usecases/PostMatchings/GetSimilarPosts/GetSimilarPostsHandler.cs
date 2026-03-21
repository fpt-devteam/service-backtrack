using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostImages;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed class GetSimilarPostsHandler : IRequestHandler<GetSimilarPostsQuery, GetSimilarPostsResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMatchRepository _postMatchRepository;
    private readonly ILlmService _llmService;
    private readonly ILogger<GetSimilarPostsHandler> _logger;

    public GetSimilarPostsHandler(
        IPostRepository postRepository,
        IPostMatchRepository postMatchRepository,
        ILlmService llmService,
        ILogger<GetSimilarPostsHandler> logger)
    {
        _postRepository = postRepository;
        _postMatchRepository = postMatchRepository;
        _llmService = llmService;
        _logger = logger;
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
            {
                await TryRunAssessmentAsync(match, sourcePost, cancellationToken);
            }
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

            var (lostCtx, foundCtx) = sourcePost.PostType == PostType.Lost
                ? (BuildContext(sourcePost), BuildContext(targetPost))
                : (BuildContext(targetPost), BuildContext(sourcePost));

            var scores = new PostMatchScores
            {
                DescriptionScore = match.DescriptionScore,
                VisualScore = match.VisualScore,
                LocationScore = match.LocationScore,
                TimeWindowScore = match.TimeWindowScore,
            };

            var assessment = await _llmService.AssessPostMatchAsync(
                lostCtx, foundCtx, scores, match.DistanceMeters, cancellationToken);

            match.IsAssessed = true;
            match.AssessmentSummary = assessment.Summary;
            match.CriteriaAssessment = assessment.Criteria;
            match.UpdatedAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lazy LLM assessment failed for PostMatch {MatchId}.", match.Id);
        }
    }

    private static PostMatchContext BuildContext(Post post)
    {
        var firstImage = post.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault();
        return new PostMatchContext
        {
            ItemName = post.ItemName,
            Description = post.Description,
            EventTime = post.EventTime,
            DisplayAddress = post.DisplayAddress,
            ImageBase64 = firstImage?.Base64Data,
            ImageMimeType = firstImage?.MimeType
        };
    }

    private static SimilarPostItem ToSimilarPostItem(PostMatch match, Post sourcePost)
    {
        var targetPost = sourcePost.PostType == PostType.Lost ? match.FoundPost : match.LostPost;

        var criteria = new SimilarPostCriteria
        {
            VisualAnalysis = new CriterionResult
            {
                Score = match.VisualScore,
                Points = match.CriteriaAssessment?.VisualAnalysis
            },
            Description = new CriterionResult
            {
                Score = match.DescriptionScore,
                Points = match.CriteriaAssessment?.Description
            },
            Location = new CriterionResult
            {
                Score = match.LocationScore,
                Points = match.CriteriaAssessment?.Location
            },
            TimeWindow = new CriterionResult
            {
                Score = match.TimeWindowScore,
                Points = match.CriteriaAssessment?.TimeWindow
            }
        };

        return new SimilarPostItem
        {
            Id = targetPost.Id,
            PostType = targetPost.PostType.ToString(),
            ItemName = targetPost.ItemName,
            Description = targetPost.Description,
            Images = targetPost.Images.Select(i => i.ToPostImageResult()).ToList(),
            Location = targetPost.Location,
            ExternalPlaceId = targetPost.ExternalPlaceId,
            DisplayAddress = targetPost.DisplayAddress,
            EventTime = targetPost.EventTime,
            MatchScore = match.MatchScore,
            DistanceMeters = match.DistanceMeters,
            MatchingLevel = match.MatchingLevel,
            IsAssessed = match.IsAssessed,
            AssessmentSummary = match.AssessmentSummary,
            Criteria = criteria
        };
    }
}
