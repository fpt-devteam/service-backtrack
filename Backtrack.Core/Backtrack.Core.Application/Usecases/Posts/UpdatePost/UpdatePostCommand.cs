using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.UpdatePost;

public sealed record UpdatePostCommand : IRequest<PostResult>
{
    public Guid PostId { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public string? PostType { get; init; }
    public string? ItemName { get; init; }
    public string? Description { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string[]? ImageUrls { get; init; }
    public LocationDto? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public DateTimeOffset? EventTime { get; init; }
}
