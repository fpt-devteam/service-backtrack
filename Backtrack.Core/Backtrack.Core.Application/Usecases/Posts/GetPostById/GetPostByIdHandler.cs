using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostById;

public sealed class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, PostResult>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostResult> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(query.PostId);
        if (post == null) throw new NotFoundException(PostErrors.NotFound);

        return new PostResult
        {
            Id = post.Id,
            Author = post.Author?.ToAuthorResult(),
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            Images = post.Images.Select(i => i.ToPostImageResult()).ToList(),
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
