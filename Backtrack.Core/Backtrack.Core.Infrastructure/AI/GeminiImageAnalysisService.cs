using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Usecases.Posts;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiImageAnalysisService(ILlmService llmService) : IImageAnalysisService
{
    // ── PersonalBelongings ──────────────────────────────────────────────────
    private const string PersonalBelongingSystemPrompt = """
        You are an AI assistant for a lost-and-found platform analyzing personal belonging items
        (wallets, bags, clothing, keys, jewelry, etc.).

        Analyze the image and extract:
        {
            "color":           "primary color(s)",
            "brand":           "brand name if visible, or null",
            "material":        "material type (leather, fabric, metal, plastic…), or null",
            "size":            "size estimate (small/medium/large or dimensions), or null",
            "condition":       "new/used/worn/damaged, or null",
            "distinctiveMarks":"unique features, stickers, scratches, patterns, or null",
            "aiDescription":   "2-3 sentence detailed description of the item"
        }
        Only describe what is visible. Respond ONLY with the JSON object, no markdown.
        """;

    // ── Electronics ────────────────────────────────────────────────────────
    private const string ElectronicSystemPrompt = """
        You are an AI assistant for a lost-and-found platform analyzing electronic devices
        (phones, laptops, tablets, headphones, chargers, etc.).

        Analyze the image and extract:
        {
            "brand":                 "device brand (Apple, Samsung, Sony…), or null",
            "model":                 "model name/number if identifiable, or null",
            "color":                 "primary color",
            "hasCase":               true/false/null,
            "caseDescription":       "case color/type if present, or null",
            "screenCondition":       "perfect/scratched/cracked/unknown, or null",
            "lockScreenDescription": "describe lock screen wallpaper/content if visible, or null",
            "distinguishingFeatures":"stickers, dents, engravings, or null",
            "aiDescription":         "2-3 sentence detailed description of the device"
        }
        Only describe what is visible. Respond ONLY with the JSON object, no markdown.
        """;

    // ── Others ─────────────────────────────────────────────────────────────
    private const string OtherSystemPrompt = """
        You are an AI assistant for a lost-and-found platform analyzing miscellaneous items.

        Analyze the image and extract:
        {
            "itemIdentifier": "concise name of what this item IS (e.g. 'Sapiens book', 'hugging pillow', 'red umbrella')",
            "primaryColor":   "primary color, or null",
            "aiDescription":  "2-3 sentence detailed description of the item"
        }
        Only describe what is visible. Respond ONLY with the JSON object, no markdown.
        """;

    public async Task<PersonalBelongingDetailInput> AnalyzePersonalBelongingAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default)
    {
        var dto = await llmService.CompleteAsync<PersonalBelongingDto>(new LlmRequest
        {
            SystemPrompt  = PersonalBelongingSystemPrompt,
            UserPrompt    = "Analyze this personal belonging item.",
            ImageBase64   = imageBase64,
            ImageMimeType = mimeType,
            Temperature   = 0.2f,
            MaxOutputTokens = 512
        }, cancellationToken);

        return new PersonalBelongingDetailInput
        {
            Color            = dto.Color,
            Brand            = dto.Brand,
            Material         = dto.Material,
            Size             = dto.Size,
            Condition        = dto.Condition,
            DistinctiveMarks = dto.DistinctiveMarks,
            AdditionalDetails = dto.AiDescription   // AI description goes into AdditionalDetails for now
        };
    }

    public async Task<ElectronicDetailInput> AnalyzeElectronicAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default)
    {
        var dto = await llmService.CompleteAsync<ElectronicDto>(new LlmRequest
        {
            SystemPrompt  = ElectronicSystemPrompt,
            UserPrompt    = "Analyze this electronic device.",
            ImageBase64   = imageBase64,
            ImageMimeType = mimeType,
            Temperature   = 0.2f,
            MaxOutputTokens = 512
        }, cancellationToken);

        return new ElectronicDetailInput
        {
            Brand                  = dto.Brand,
            Model                  = dto.Model,
            Color                  = dto.Color,
            HasCase                = dto.HasCase,
            CaseDescription        = dto.CaseDescription,
            ScreenCondition        = dto.ScreenCondition,
            LockScreenDescription  = dto.LockScreenDescription,
            DistinguishingFeatures = dto.DistinguishingFeatures,
            AdditionalDetails      = dto.AiDescription
        };
    }

    public async Task<OtherDetailInput> AnalyzeOtherAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default)
    {
        var dto = await llmService.CompleteAsync<OtherDto>(new LlmRequest
        {
            SystemPrompt  = OtherSystemPrompt,
            UserPrompt    = "Analyze this item.",
            ImageBase64   = imageBase64,
            ImageMimeType = mimeType,
            Temperature   = 0.2f,
            MaxOutputTokens = 512
        }, cancellationToken);

        return new OtherDetailInput
        {
            ItemIdentifier = dto.ItemIdentifier ?? "Unknown item",
            PrimaryColor   = dto.PrimaryColor,
            Notes          = dto.AiDescription
        };
    }

    // ── Private DTOs ────────────────────────────────────────────────────────
    private sealed class PersonalBelongingDto
    {
        [JsonPropertyName("color")]            public string? Color { get; set; }
        [JsonPropertyName("brand")]            public string? Brand { get; set; }
        [JsonPropertyName("material")]         public string? Material { get; set; }
        [JsonPropertyName("size")]             public string? Size { get; set; }
        [JsonPropertyName("condition")]        public string? Condition { get; set; }
        [JsonPropertyName("distinctiveMarks")] public string? DistinctiveMarks { get; set; }
        [JsonPropertyName("aiDescription")]    public string? AiDescription { get; set; }
    }

    private sealed class ElectronicDto
    {
        [JsonPropertyName("brand")]                  public string? Brand { get; set; }
        [JsonPropertyName("model")]                  public string? Model { get; set; }
        [JsonPropertyName("color")]                  public string? Color { get; set; }
        [JsonPropertyName("hasCase")]                public bool? HasCase { get; set; }
        [JsonPropertyName("caseDescription")]        public string? CaseDescription { get; set; }
        [JsonPropertyName("screenCondition")]        public string? ScreenCondition { get; set; }
        [JsonPropertyName("lockScreenDescription")]  public string? LockScreenDescription { get; set; }
        [JsonPropertyName("distinguishingFeatures")] public string? DistinguishingFeatures { get; set; }
        [JsonPropertyName("aiDescription")]          public string? AiDescription { get; set; }
    }

    private sealed class OtherDto
    {
        [JsonPropertyName("itemIdentifier")] public string? ItemIdentifier { get; set; }
        [JsonPropertyName("primaryColor")]   public string? PrimaryColor { get; set; }
        [JsonPropertyName("aiDescription")]  public string? AiDescription { get; set; }
    }
}
