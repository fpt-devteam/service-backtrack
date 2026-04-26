using System.Text.Json.Serialization;

namespace Backtrack.Core.Domain.ValueObjects;

public sealed record NotificationLocation
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }
}

public sealed record NotificationData
{
    [JsonPropertyName("screenPath")]
    public string? ScreenPath { get; init; }

    [JsonPropertyName("location")]
    public NotificationLocation? Location { get; init; }

    [JsonPropertyName("displayAddress")]
    public string? DisplayAddress { get; init; }
}
