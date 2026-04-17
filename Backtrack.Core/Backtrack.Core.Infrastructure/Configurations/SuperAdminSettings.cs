namespace Backtrack.Core.Infrastructure.Configurations;

public sealed class SuperAdminSettings
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string DisplayName { get; init; }
}
