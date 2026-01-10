using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using Backtrack.Core.Application.Common.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Common.Interfaces.Helpers;
using Backtrack.Core.Application.Posts.Commands.UpdatePostContentEmbedding;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Application.Users;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Posts.Commands.CreatePost;

public sealed class CreatePostHandler : IRequestHandler<CreatePostCommand, PostResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHasher _hasher;
    private readonly IBackgroundJobService _backgroundJobService;

    public CreatePostHandler(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IHasher hasher,
        IBackgroundJobService backgroundJobService)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _hasher = hasher;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<PostResult> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        GeoPoint? location = null;
        if (command.Location is not null)
        {
            location = new GeoPoint(command.Location.Latitude, command.Location.Longitude);
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = command.AuthorId,
            PostType = command.PostType,
            ItemName = command.ItemName,
            Description = command.Description,
            DistinctiveMarks = command.DistinctiveMarks,
            ImageUrls = command.ImageUrls,
            Location = location,
            ExternalPlaceId = command.ExternalPlaceId,
            DisplayAddress = command.DisplayAddress,
            ContentEmbedding = null, // Will be generated asynchronously
            ContentEmbeddingStatus = ContentEmbeddingStatus.Pending,
            ContentHash = command.DistinctiveMarks != null
                ? _hasher.HashStrings(command.ItemName, command.Description, command.DistinctiveMarks)
                : _hasher.HashStrings(command.ItemName, command.Description),
            EventTime = command.EventTime,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _postRepository.CreateAsync(post);
        await _postRepository.SaveChangesAsync();

        // Enqueue background job to generate content embedding
        _backgroundJobService.EnqueueJob(
            new UpdatePostContentEmbeddingCommand(post.Id));

        // Fetch author information
        var author = await _userRepository.GetByIdAsync(post.AuthorId);
        if (author == null)
        {
            throw new NotFoundException(new Error("AuthorNotFound", "Author not found"));
        }

        return new PostResult
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            Author = new AuthorResult
            {
                Id = author.Id,
                DisplayName = author.DisplayName,
                AvatarUrl = author.AvatarUrl
            },
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            ImageUrls = post.ImageUrls,
            Location = post.Location != null
                ? new LocationResult
                {
                    Latitude = post.Location.Latitude,
                    Longitude = post.Location.Longitude
                }
                : null,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
