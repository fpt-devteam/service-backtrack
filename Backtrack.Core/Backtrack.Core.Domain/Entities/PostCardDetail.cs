namespace Backtrack.Core.Domain.Entities;

public sealed class PostCardDetail
{
    public required Guid PostId { get; set; }

    // Hash-only sensitive fields (SHA256 + PEPPER)
    public string? CardNumberHash { get; set; }
    public string? CardNumberMasked { get; set; }       // "***8901" for display

    // Plain fields (needed for fuzzy matching)
    public string? HolderName { get; set; }
    public string? HolderNameNormalized { get; set; }   // lowercase, no diacritics
    public DateOnly? DateOfBirth { get; set; }

    // Other card metadata
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }

    // OCR dump from card images
    public string? OcrText { get; set; }

    public string? AdditionalDetails { get; set; }

    // AI-extracted description (for embedding context)
    public string? AiDescription { get; set; }

    public string? ContentHash { get; set; }

    public Post Post { get; set; } = default!;
}
