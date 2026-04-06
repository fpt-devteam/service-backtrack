using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Users;

public sealed record UserResult
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public bool ShowEmail { get; init; }
    public bool ShowPhone { get; init; }
    public required UserGlobalRole GlobalRole { get; init; }
    public required UserStatus Status { get; init; }
}

public static class UserResultMapper
{
    public static UserResult ToUserResult(this User user)
    {
        return new UserResult
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            ShowEmail = user.ShowEmail,
            ShowPhone = user.ShowPhone,
            GlobalRole = user.GlobalRole,
            Status = user.Status,
        };
    }
}

