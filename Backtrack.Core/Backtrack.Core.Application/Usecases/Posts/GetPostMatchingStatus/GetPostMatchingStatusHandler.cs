using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostMatchingStatus;

public sealed class GetPostMatchingStatusHandler(IPostRepository postRepository)
    : IRequestHandler<GetPostMatchingStatusQuery, GetPostMatchingStatusResult>
{
    public async Task<GetPostMatchingStatusResult> Handle(GetPostMatchingStatusQuery query, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(query.PostId, isTrack: false)
            ?? throw new NotFoundException(PostErrors.NotFound);

        return new GetPostMatchingStatusResult
        {
            PostId = post.Id,
            EmbeddingStatus = post.ContentEmbeddingStatus.ToString(),
            MatchingStatus = post.PostMatchingStatus.ToString(),
        };
    }
}
