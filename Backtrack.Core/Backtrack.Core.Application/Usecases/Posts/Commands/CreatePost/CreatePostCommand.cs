using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Commands.CreatePost;

public sealed record CreatePostCommand : IRequest<PostResult>
{
    public string AuthorId { get; init; } = string.Empty;
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public LocationDto? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
}

public sealed record LocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
