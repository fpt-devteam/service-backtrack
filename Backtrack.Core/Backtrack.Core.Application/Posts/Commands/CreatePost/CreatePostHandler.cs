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

    public async Task<PostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        // Parse PostType
        if (!Enum.TryParse<PostType>(request.PostType, out var postType))
        {
            throw new ValidationException(PostErrors.InvalidPostType);
        }

        // Create location point if provided
        GeoPoint? location = null;
        if (request.Location != null)
        {
            location = new GeoPoint(request.Location.Latitude, request.Location.Longitude);
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            PostType = postType,
            ItemName = request.ItemName,
            Description = request.Description,
            Material = request.Material,
            Brands = request.Brands,
            Colors = request.Colors,
            ImageUrls = request.ImageUrls,
            Location = location,
            EventTime = request.EventTime,
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
            Material = post.Material,
            Brands = post.Brands,
            Colors = post.Colors,
            ImageUrls = post.ImageUrls,
            Location = post.Location != null
                ? new LocationResult
                {
                    Latitude = post.Location.Latitude,
                    Longitude = post.Location.Longitude
                }
                : null,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
