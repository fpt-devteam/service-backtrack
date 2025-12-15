using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.Contract.Posts.Requests;

public sealed record CreatePostRequest
{
    [Required]
    public required string PostType { get; init; }
    [Required]
    public required string ItemName { get; init; }
    [Required]
    public required string Description { get; init; }
    public string[] Material { get; init; } = Array.Empty<string>();
    public string[] Brands { get; init; } = Array.Empty<string>();
    public string[] Colors { get; init; } = Array.Empty<string>();
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public LocationRequest? Location { get; init; }
    [Required]
    public required DateTimeOffset EventTime { get; init; }
}

public sealed record LocationRequest
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
