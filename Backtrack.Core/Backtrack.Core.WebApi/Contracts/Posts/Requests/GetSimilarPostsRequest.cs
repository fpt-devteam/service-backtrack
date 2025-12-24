using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.WebApi.Contracts.Posts.Requests;

/// <summary>
/// Request to get posts similar to a specific post
/// </summary>
public sealed record GetSimilarPostsRequest
{
    /// <summary>
    /// ID of the post to find similar items for
    /// </summary>
    [Required]
    public Guid PostId { get; init; }

    /// <summary>
    /// Maximum number of similar posts to return (default: 20, max: 50)
    /// </summary>
    [Range(1, 50)]
    public int Limit { get; init; } = 20;
}
