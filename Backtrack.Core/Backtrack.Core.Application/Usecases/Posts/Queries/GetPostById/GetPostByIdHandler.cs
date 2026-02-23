using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Queries.GetPostById;

public sealed class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, PostResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public GetPostByIdHandler(IPostRepository postRepository, IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task<PostResult> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(query.PostId);

        if (post == null)
        {
            throw new NotFoundException(PostErrors.NotFound);
        }

        var author = await _userRepository.GetByIdAsync(post.AuthorId);
        if (author == null)
        {
            throw new NotFoundException(UserErrors.NotFound);
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
