using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostMatchingStatus;

public sealed record GetPostMatchingStatusQuery : IRequest<GetPostMatchingStatusResult>
{
    public required Guid PostId { get; init; }
}
