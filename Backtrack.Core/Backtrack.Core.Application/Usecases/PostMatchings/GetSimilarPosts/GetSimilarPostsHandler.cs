using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;

public sealed class GetSimilarPostsHandler : IRequestHandler<GetSimilarPostsQuery, GetSimilarPostsResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMatchRepository _postMatchRepository;
    private readonly ILogger<GetSimilarPostsHandler> _logger;

    public GetSimilarPostsHandler(
        IPostRepository postRepository,
        IPostMatchRepository postMatchRepository,
        ILogger<GetSimilarPostsHandler> logger)
    {
        _postRepository      = postRepository;
        _postMatchRepository = postMatchRepository;
        _logger              = logger;
    }

    public async Task<GetSimilarPostsResult> Handle(GetSimilarPostsQuery request, CancellationToken cancellationToken)
    {
        _ = await _postRepository.GetByIdAsync(request.PostId)
            ?? throw new NotFoundException(PostErrors.NotFound);

        var matches = (await _postMatchRepository.GetMatchesByPostIdAsync(request.PostId, cancellationToken))
            .Take(request.Limit)
            .ToList();

        var results = matches.Select(ToSimilarPostItem).ToList();
        return new GetSimilarPostsResult { SimilarPosts = results };
    }

    private static SimilarPostItem ToSimilarPostItem(Domain.Entities.PostMatch match)
    {
        var target = match.CandidatePost;

        return new SimilarPostItem
        {
            Id               = target.Id,
            PostTitle        = target.PostTitle,
            PostType         = target.PostType,
            Category         = target.Category,
            SubcategoryId    = target.SubcategoryId,
            PersonalBelongingDetail = target.PersonalBelongingDetail,
            CardDetail       = target.CardDetail,
            ElectronicDetail = target.ElectronicDetail,
            OtherDetail      = target.OtherDetail,
            ImageUrls        = target.ImageUrls,
            Location         = target.Location,
            ExternalPlaceId  = target.ExternalPlaceId,
            DisplayAddress   = target.DisplayAddress,
            EventTime        = target.EventTime,
            Score    = match.Score,
            Evidence = match.Evidence,
            Status   = match.Status
        };
    }
}
