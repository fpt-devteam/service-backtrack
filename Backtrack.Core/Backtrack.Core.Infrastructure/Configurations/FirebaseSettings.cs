namespace Backtrack.Core.Infrastructure.Configurations;

public sealed class FirebaseSettings
{
    public required string ServiceAccountJsonBase64 { get; init; }
    public string? ProjectId { get; init; }
}
