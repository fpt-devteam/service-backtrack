using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostById;

public sealed class GetPostByIdHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository) : IRequestHandler<GetPostByIdQuery, PostResult>
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

        var receiveReport = post.OrganizationId.HasValue
            ? await receiveReportRepository.GetByPostIdAsync(post.Id, cancellationToken)
            : null;

        return post.ToPostResult() with { FinderInfo = receiveReport?.FinderInfo };
    }
}
