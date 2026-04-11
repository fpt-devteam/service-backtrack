namespace Backtrack.Core.Domain.ValueObjects;

public sealed record QrGradientSettings
{
    public required bool Enabled { get; init; }
    public required string[] Colors { get; init; } // exactly 2 hex colors
    public required string Direction { get; init; } // "to-r" | "to-b" | "to-br" | "to-tr"
}
