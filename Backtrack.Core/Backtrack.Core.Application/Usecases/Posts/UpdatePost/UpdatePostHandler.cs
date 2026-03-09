using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Application.Usecases.Posts.UpdatePostContentEmbedding;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.UpdatePost;

public sealed class UpdatePostHandler : IRequestHandler<UpdatePostCommand, PostResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBackgroundJobService _backgroundJobService;

    public UpdatePostHandler(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IBackgroundJobService backgroundJobService)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<PostResult> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, true);
        if (post == null) throw new NotFoundException(PostErrors.NotFound);
        if (post.AuthorId != command.AuthorId) throw new ForbiddenException(PostErrors.Forbidden);
        bool needsReEmbedding = false;

        if (command.PostType != null)
        {
            if (!Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out var postType)) throw new ValidationException(PostErrors.InvalidPostType);
            if (post.PostType != postType)
            {
                post.PostType = postType;
                needsReEmbedding = true;
            }
        }

        if (command.ItemName != null && post.ItemName != command.ItemName)
        {
            post.ItemName = command.ItemName;
            needsReEmbedding = true;
        }

        if (command.Description != null && post.Description != command.Description)
        {
            post.Description = command.Description;
            needsReEmbedding = true;
        }

        if (command.DistinctiveMarks != null && post.DistinctiveMarks != command.DistinctiveMarks)
        {
            post.DistinctiveMarks = command.DistinctiveMarks;
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

        post.ImageUrls = command.ImageUrls ?? post.ImageUrls;
        post.ExternalPlaceId = command.ExternalPlaceId ?? post.ExternalPlaceId;
        post.DisplayAddress = command.DisplayAddress ?? post.DisplayAddress;
        post.EventTime = command.EventTime.HasValue ? command.EventTime.Value : post.EventTime;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        if (needsReEmbedding) post.ContentEmbeddingStatus = ContentEmbeddingStatus.Pending;
        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync();

        if (needsReEmbedding) _backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return new PostResult
        {
            Id = post.Id,
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
