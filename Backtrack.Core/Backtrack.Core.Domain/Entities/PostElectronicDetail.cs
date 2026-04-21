namespace Backtrack.Core.Domain.Entities;

public sealed class PostElectronicDetail
{
    public required Guid PostId { get; set; }

    // Concise device name (e.g. "iPhone 14 Pro", "Sony WH-1000XM5")
    public required string ItemName { get; set; }

    // Structured fields (strong matching signals)
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }

    // Device-specific (nullable for accessories)
    public bool? HasCase { get; set; }
    public string? CaseDescription { get; set; }
    public string? ScreenCondition { get; set; }        // "perfect", "scratched", "cracked"
    public string? LockScreenDescription { get; set; }

    public string? DistinguishingFeatures { get; set; }

    // AI-generated description
    public string? AiDescription { get; set; }

    public string? AdditionalDetails { get; set; }

    public string? ContentHash { get; set; }

    public Post Post { get; set; } = default!;
}
