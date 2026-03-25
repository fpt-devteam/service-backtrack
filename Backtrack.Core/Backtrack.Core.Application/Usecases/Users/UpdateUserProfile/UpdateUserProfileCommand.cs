using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Users.UpdateUserProfile;

public sealed record UpdateUserProfileCommand : IRequest<UserResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public bool? ShowEmail { get; init; }
    public bool? ShowPhone { get; init; }
}
