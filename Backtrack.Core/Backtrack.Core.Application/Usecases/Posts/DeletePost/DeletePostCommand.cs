using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.DeletePost;

public sealed record DeletePostCommand : IRequest
{
    [JsonIgnore]
    public required Guid PostId { get; init; }
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
}
