using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed record ImageInput
{
    public required string Url { get; init; }
    public required string Base64Data { get; init; }
    public required string MimeType { get; init; }
    public string? FileName { get; init; }
    public long? FileSizeBytes { get; init; }
}

public sealed record CreatePostCommand : IRequest<PostResult>
{
    public string AuthorId { get; init; } = string.Empty;
    public Guid? OrganizationId { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public ImageInput[] Images { get; init; } = Array.Empty<ImageInput>();
    public required GeoPoint Location { get; init; }
    public required string DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public required DateTimeOffset EventTime { get; init; }
}
