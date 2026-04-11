namespace Backtrack.Core.Domain.ValueObjects;

public sealed record QrLogoSettings
{
    public required string Url { get; init; }
    public required int Size { get; init; }
    public required int Margin { get; init; }
    public required int BorderRadius { get; init; }
    public required string BackgroundColor { get; init; }
}
