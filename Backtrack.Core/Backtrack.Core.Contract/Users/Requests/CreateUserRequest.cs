using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.Contract.Users.Requests;

public sealed record CreateUserRequest
{
    [Required]
    public required string UserId { get; init; }
    [Required]
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
}