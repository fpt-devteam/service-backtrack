using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetFeed;

public sealed record GetFeedQuery : IRequest<List<FeedPostResult>>
{
    public required GeoPoint Location { get; init; }
}
