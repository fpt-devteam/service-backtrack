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
    public PostItem? Item { get; init; }
    public GeoPoint? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public string[]? ImageUrls { get; init; }
    public DateTimeOffset? EventTime { get; init; }
}
