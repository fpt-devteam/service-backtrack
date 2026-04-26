namespace Backtrack.Core.Domain.Entities;

public sealed class PostCardDetail
{
    public required Guid PostId { get; set; }
    // Human-readable card name (e.g. "FPT Polytechnic Student Card", "Vietnam National ID")
    public required string ItemName { get; set; }

    // Hash-only sensitive fields (SHA256 + PEPPER)
    public string? CardNumberHash { get; set; }
    public string? CardNumberMasked { get; set; }       // "***8901" for display

    // Plain fields (needed for fuzzy matching)
    public string? HolderName { get; set; }
    public string? HolderNameNormalized { get; set; }   // lowercase, no diacritics

    // Other card metadata
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }

    public string? AdditionalDetails { get; set; }

    // OCR dump from card images
    public string? OcrText { get; set; }

    public string? ContentHash { get; set; }

    public Post Post { get; set; } = default!;

    [Obsolete("Use OcrText and AdditionalDetails instead for better structure and searchability")]
    public string? AiDescription { get; set; }
}
