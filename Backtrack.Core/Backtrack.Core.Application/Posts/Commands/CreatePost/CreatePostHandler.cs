using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Posts.Commands.CreatePost;

public sealed class CreatePostHandler : IRequestHandler<CreatePostCommand, PostResult>
{
    private readonly IPostRepository _postRepository;

    public CreatePostHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
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
            PostType = command.PostType,
            ItemName = command.ItemName,
            Description = command.Description,
            ImageUrls = command.ImageUrls,
            Location = location,
            ExternalPlaceId = command.ExternalPlaceId,
            DisplayAddress = command.DisplayAddress,
            EventTime = command.EventTime,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _postRepository.CreateAsync(post);
        await _postRepository.SaveChangesAsync();

        return new PostResult
        {
            Id = post.Id,
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
