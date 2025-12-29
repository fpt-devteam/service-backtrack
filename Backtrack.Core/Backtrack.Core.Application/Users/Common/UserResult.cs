using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Users.Common;

public sealed record UserResult
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public required UserGlobalRole GlobalRole { get; init; }

}
