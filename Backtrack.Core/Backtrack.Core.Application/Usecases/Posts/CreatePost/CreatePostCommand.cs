using System.Text.Json.Serialization;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed record CreatePostCommand : IRequest<PostResult>
{
    [JsonIgnore]
    public string AuthorId { get; init; } = string.Empty;
    [JsonIgnore]
    public Guid? OrganizationId { get; init; }
    public string? PostType { get; init; }
    public required PostItem Item { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public GeoPoint? Location { get; init; }
    public string? DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public DateTimeOffset? EventTime { get; init; }

    // Only used when creating an org inventory item — captures who brought the item in
    public FinderInfo? FinderInfo { get; init; }
}
