using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.UpdatePost;

public sealed record UpdatePostCommand : IRequest<PostResult>
{
    public Guid PostId { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public Guid? OrganizationId { get; init; }
    public string? PostType { get; init; }
    public string? ItemName { get; init; }
    public string? Description { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string[]? ImageUrls { get; init; }
    public GeoPoint? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public DateTimeOffset? EventTime { get; init; }
}
