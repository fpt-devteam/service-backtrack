namespace Backtrack.Core.WebApi.Contracts.Users.Requests;

public class EnsureUserExistRequest
{
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}
