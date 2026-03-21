using MediatR;

namespace Backtrack.Core.Application.Usecases.PostMatchings.GetPostMatchingStatus;

public sealed record GetPostMatchingStatusQuery : IRequest<GetPostMatchingStatusResult>
{
    public required Guid PostId { get; init; }
}
