using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Backtrack.Core.Application.Usecases.PostMatchings;

namespace Backtrack.Core.Application.Usecases.Posts.UpdatePost;

public sealed class UpdatePostHandler : IRequestHandler<UpdatePostCommand, PostResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IHasher _hasher;

    public UpdatePostHandler(
        IPostRepository postRepository,
        IBackgroundJobService backgroundJobService,
        IHasher hasher)
    {
        _postRepository = postRepository;
        _backgroundJobService = backgroundJobService;
        _hasher = hasher;
    }

    public async Task<PostResult> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, true);
        if (post == null) throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId.HasValue) throw new ForbiddenException(PostErrors.Forbidden);
        if (post.AuthorId != command.UserId) throw new ForbiddenException(PostErrors.Forbidden);

        bool needsReEmbedding = false;

        if (command.PostType != null && !Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out var postType))
            throw new ValidationException(PostErrors.InvalidPostType);

        // Update category-specific detail if provided
        if (command.PersonalBelongingDetail != null)
        {
            UpdatePersonalBelongingDetail(post, command.PersonalBelongingDetail);
            post.PostTitle = post.PersonalBelongingDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        else if (command.CardDetail != null)
        {
            UpdateCardDetail(post, command.CardDetail, _hasher);
            post.PostTitle = post.CardDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        else if (command.ElectronicDetail != null)
        {
            UpdateElectronicDetail(post, command.ElectronicDetail);
            post.PostTitle = post.ElectronicDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        if (command.OtherDetail != null)
        {
            UpdateOtherDetail(post, command.OtherDetail);
            post.PostTitle = post.OtherDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }

        if (command.PostTitle != null && post.PostTitle != command.PostTitle)
        {
            post.PostTitle = command.PostTitle;
            needsReEmbedding = true;
        }

        if (command.Location != null)
        {
            var newLocation = new GeoPoint(command.Location.Latitude, command.Location.Longitude);
            if (post.Location == null ||
                DoubleUtil.AreNotApproximatelyEqual(post.Location.Latitude, newLocation.Latitude) ||
                DoubleUtil.AreNotApproximatelyEqual(post.Location.Longitude, newLocation.Longitude))
            {
                post.Location = newLocation;
                needsReEmbedding = true;
            }
        }

        post.ExternalPlaceId = command.ExternalPlaceId ?? post.ExternalPlaceId;
        post.DisplayAddress = command.DisplayAddress ?? post.DisplayAddress;

        if (command.ImageUrls != null)
        {
            post.ImageUrls = command.ImageUrls.ToList();
            needsReEmbedding = true;
        }

        post.EventTime = command.EventTime.HasValue ? command.EventTime.Value : post.EventTime;

        if (command.Status is not null && Enum.TryParse<PostStatus>(command.Status, ignoreCase: true, out var parsedStatus))
            post.Status = parsedStatus;

        post.UpdatedAt = DateTimeOffset.UtcNow;

        if (needsReEmbedding)
        {
            post.EmbeddingStatus = EmbeddingStatus.Pending;
            post.PostMatchingStatus = PostMatchingStatus.Pending;
        }
        await _postRepository.SaveChangesAsync();

        if (needsReEmbedding)
            _backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(
                orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return post.ToPostResult();
    }

    private static void UpdatePersonalBelongingDetail(Post post, PersonalBelongingDetailDto input)
    {
        if (post.PersonalBelongingDetail is { } d)
            input.ApplyTo(d);
        else
            post.PersonalBelongingDetail = input.ToEntity(post.Id);
    }

    private static void UpdateCardDetail(Post post, CardDetailDto input, IHasher hasher)
    {
        if (post.CardDetail is { } d)
            input.ApplyTo(d, hasher);
        else
            post.CardDetail = input.ToEntity(post.Id, hasher);
    }

    private static void UpdateElectronicDetail(Post post, ElectronicDetailDto input)
    {
        if (post.ElectronicDetail is { } d)
            input.ApplyTo(d);
        else
            post.ElectronicDetail = input.ToEntity(post.Id);
    }

    private static void UpdateOtherDetail(Post post, OtherDetailDto input)
    {
        if (post.OtherDetail is { } d)
            input.ApplyTo(d);
        else
            post.OtherDetail = input.ToEntity(post.Id);
    }
}
