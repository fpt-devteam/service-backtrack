using Backtrack.Core.Application.Posts.Common;
using MediatR;

namespace Backtrack.Core.Application.Posts.Commands.CreatePost;

public sealed record CreatePostCommand : IRequest<PostResult>
{
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string[] Material { get; init; } = Array.Empty<string>();
    public string[] Brands { get; init; } = Array.Empty<string>();
    public string[] Colors { get; init; } = Array.Empty<string>();
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public LocationDto? Location { get; init; }
    public required DateTimeOffset EventTime { get; init; }
}

public sealed record LocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
