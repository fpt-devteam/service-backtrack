using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class User : Entity<string>
{
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public UserGlobalRole GlobalRole { get; set; } = UserGlobalRole.Customer;
}