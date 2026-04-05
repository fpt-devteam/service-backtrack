using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostById;

public sealed class GetPostByIdHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<GetPostByIdQuery, PostResult>
{
    public async Task<PostResult> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(query.PostId)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (query.UserId is not null && post.OrganizationId.HasValue)
        {
            var membership = await membershipRepository.GetByOrgAndUserAsync(post.OrganizationId.Value, query.UserId, cancellationToken);
            if (membership is null) throw new ForbiddenException(PostErrors.Forbidden);
        }

        return new PostResult
        {
            Id              = post.Id,
            Author          = post.Author?.ToPostAuthorResult(),
            Organization    = post.Organization?.ToOrganizationOnPost(),
            PostType        = post.PostType,
            Status          = post.Status,
            Item            = post.Item,
            ImageUrls       = post.ImageUrls,
            Location        = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress  = post.DisplayAddress,
            EventTime       = post.EventTime,
            CreatedAt       = post.CreatedAt
        };
    }
}
