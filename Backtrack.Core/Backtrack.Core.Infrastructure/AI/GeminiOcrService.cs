using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Usecases.Posts;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiOcrService(ILlmService llmService) : IOcrService
{
    private const string SystemPrompt = """
        You are an OCR and information-extraction specialist for identification cards and documents.
        Analyze the provided card/document image and extract all visible text and structured fields.

        Respond ONLY with valid JSON — no markdown, no extra text:
        {
            "itemName":             "full descriptive name of the card (e.g. 'FPT Polytechnic Student Card', 'Vietnam National ID Card', 'Vietcombank Visa Card'), or null",
            "ocrText":              "full verbatim OCR text from the card",
            "cardNumber":           "card/ID number exactly as printed (digits and hyphens only, no spaces), or null",
            "holderName":           "full name of card holder, or null",
            "holderNameNormalized": "lowercase, no diacritics version of the name, or null",
            "issuingAuthority":     "issuing authority/organization, or null",
            "dateOfBirth":          "YYYY-MM-DD, or null",
            "issueDate":            "YYYY-MM-DD, or null",
            "expiryDate":           "YYYY-MM-DD, or null"
        }
        Set any field to null if not visible or not applicable.
        """;

    public async Task<CardDetailInput> ExtractCardTextAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        var dto = await llmService.CompleteAsync<CardOcrDto>(new LlmRequest
        {
            SystemPrompt    = SystemPrompt,
            UserPrompt      = "Extract all text and structured fields from this card image.",
            ImageBase64     = imageBase64,
            ImageMimeType   = mimeType,
            Temperature     = 0.1f,
            MaxOutputTokens = 1024
        }, cancellationToken);

        return new CardDetailInput
        {
            ItemName             = dto.ItemName,
            CardNumber           = dto.CardNumber,
            OcrText              = dto.OcrText,
            HolderName           = dto.HolderName,
            HolderNameNormalized = dto.HolderNameNormalized,
            IssuingAuthority     = dto.IssuingAuthority,
            DateOfBirth          = ParseDate(dto.DateOfBirth),
            IssueDate            = ParseDate(dto.IssueDate),
            ExpiryDate           = ParseDate(dto.ExpiryDate)
        };
    }

    private static DateOnly? ParseDate(string? value)
        => DateOnly.TryParse(value, out var d) ? d : null;

    private sealed class CardOcrDto
    {
        [JsonPropertyName("itemName")]             public string? ItemName { get; set; }
        [JsonPropertyName("ocrText")]              public string? OcrText { get; set; }
        [JsonPropertyName("cardNumber")]           public string? CardNumber { get; set; }
        [JsonPropertyName("holderName")]           public string? HolderName { get; set; }
        [JsonPropertyName("holderNameNormalized")] public string? HolderNameNormalized { get; set; }
        [JsonPropertyName("issuingAuthority")]     public string? IssuingAuthority { get; set; }
        [JsonPropertyName("dateOfBirth")]          public string? DateOfBirth { get; set; }
        [JsonPropertyName("issueDate")]            public string? IssueDate { get; set; }
        [JsonPropertyName("expiryDate")]           public string? ExpiryDate { get; set; }
    }
}
