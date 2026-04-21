using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
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
            "itemName":        "concise item name combining type, brand, and color (e.g. 'Black Louis Vuitton wallet', 'JanSport navy backpack', 'silver keyring'), or null",
            "color":           "primary color(s)",
            "brand":           "brand name if visible, or null",
            "material":        "material type (leather, fabric, metal, plastic…), or null",
            "size":            "size estimate (small/medium/large or dimensions), or null",
            "condition":       "new/used/worn/damaged, or null",
            "distinctiveMarks":"unique features, stickers, scratches, patterns, or null",
            "aiDescription":   "Write 2-3 sentences highlighting the most identifiable traits: item type, color, brand, material, and any distinctive marks. Optimized for semantic search — omit filler, focus on specifics a finder or owner would search for."
        }
        Only describe what is visible. Respond ONLY with the JSON object, no markdown.
        """;

    // ── Electronics ────────────────────────────────────────────────────────
    private const string ElectronicSystemPrompt = """
        You are an AI assistant for a lost-and-found platform analyzing electronic devices
        (phones, laptops, tablets, headphones, chargers, etc.).

        Analyze the image and extract:
        {
            "itemName":              "concise device name combining brand, model, and color (e.g. 'Black iPhone 14 Pro', 'Silver MacBook Air M2', 'White AirPods Pro'), or null",
            "brand":                 "device brand (Apple, Samsung, Sony…), or null",
            "model":                 "model name/number if identifiable, or null",
            "color":                 "primary color",
            "hasCase":               true/false/null,
            "caseDescription":       "case color/type if present, or null",
            "screenCondition":       "perfect/scratched/cracked/unknown, or null",
            "lockScreenDescription": "describe lock screen wallpaper/content if visible, or null",
            "distinguishingFeatures":"stickers, dents, engravings, or null",
            "aiDescription":         "Write 2-3 sentences describing the device: device type, brand, model, color, case presence, and the single most identifying visual feature (e.g. cracked screen, unique sticker, engraving). Optimized for semantic search — be specific, skip generic phrases."
        }
        Only describe what is visible. Respond ONLY with the JSON object, no markdown.
        """;

    // ── Others ─────────────────────────────────────────────────────────────
    private const string OtherSystemPrompt = """
        You are an AI assistant for a lost-and-found platform analyzing miscellaneous items.

        Analyze the image and extract:
        {
            "itemName":      "concise name of what this item IS (e.g. 'Sapiens book', 'hugging pillow', 'red umbrella')",
            "primaryColor":  "primary color, or null",
            "aiDescription": "One concise sentence naming the item and its most searchable traits: type, color, shape, size, brand if visible, and any unique markings. Optimized for semantic search — a person searching for this lost item should be able to find it from this sentence alone."
        }
        Only describe what is visible. Respond ONLY with the JSON object, no markdown.
        """;

    private const int MaxRetries = 3;
    private const int MaxOutputTokensForAnalysis = 2048;
    private const int MaxOutputTokensForConsistency = 256;

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
            MaxOutputTokens = MaxOutputTokensForAnalysis
        }, cancellationToken);

        return new PersonalBelongingDetailInput
        {
            ItemName         = dto.ItemName ?? "Unknown personal belonging item",
            Color            = dto.Color,
            Brand            = dto.Brand,
            Material         = dto.Material,
            Size             = dto.Size,
            Condition        = dto.Condition,
            DistinctiveMarks = dto.DistinctiveMarks,
            AiDescription    = dto.AiDescription
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
            MaxOutputTokens = MaxOutputTokensForAnalysis
        }, cancellationToken);

        return new ElectronicDetailInput
        {
            ItemName               = dto.ItemName ?? "Unknown electronic device",
            Brand                  = dto.Brand,
            Model                  = dto.Model,
            Color                  = dto.Color,
            HasCase                = dto.HasCase,
            CaseDescription        = dto.CaseDescription,
            ScreenCondition        = dto.ScreenCondition,
            LockScreenDescription  = dto.LockScreenDescription,
            DistinguishingFeatures = dto.DistinguishingFeatures,
            AiDescription          = dto.AiDescription
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
            MaxOutputTokens = MaxOutputTokensForAnalysis
        }, cancellationToken);

        return new OtherDetailInput
        {
            ItemName      = dto.ItemName ?? "Unknown item",
            PrimaryColor  = dto.PrimaryColor,
            AiDescription = dto.AiDescription
        };
    }

    // ── Consistency verification ───────────────────────────────────────────
    private const string ConsistencySystemPrompt = """
        You are a validation assistant for a lost-and-found platform.
        You will receive one or more images and an expected subcategory code.

        Your task:
        Determine whether the item(s) in the images belong to the expected subcategory.

        Available subcategories (use these exact codes in your response):

        Category: Electronics
          phone, laptop, smartwatch, charger_adapter, mouse, keyboard, powerbank, power_outlet, headphone, earphone

        Category: Cards
          identification_card, passport, driver_license, personal_card, bank_card, student_card, company_card

        Category: PersonalBelongings
          wallets, keys, suitcases, backpack, clothings, jewelry

        Category: Others (exception/fallback — use ONLY when the item clearly does not fit any category above)
          others

        Rules:
        - "matchesSubcategory" is false if the item does not belong to the expected subcategory code.
        - "suggestedSubcategoryCode" must be the EXACT code from the list above that best fits the item in the images.
          Set it even when "matchesSubcategory" is true (confirm the correct code).
          Use "others" if the item does not fit any specific subcategory above. Only set null if no item is identifiable at all.
        - "reason" must be one clear sentence explaining the result, and if wrong must tell the user which subcategory to pick.

        Respond ONLY with JSON (no markdown):
        {
            "matchesSubcategory": true or false,
            "suggestedSubcategoryCode": "exact_code or null",
            "reason": "one sentence explanation"
        }
        """;

    public async Task<ItemConsistencyResult> VerifyItemConsistencyAsync(
        IReadOnlyList<FetchedImage> images,
        string subcategoryName,
        CancellationToken cancellationToken = default)
    {
        var llmImages = images
            .Select(img => new LlmImage(img.Base64, img.MimeType))
            .ToList();

        var dto = await llmService.CompleteAsync<ConsistencyDto>(new LlmRequest
        {
            SystemPrompt    = ConsistencySystemPrompt,
            UserPrompt      = $"Expected subcategory: \"{subcategoryName}\". Verify the image(s) against this subcategory.",
            Images          = llmImages,
            Temperature     = 0.1f,
            MaxOutputTokens = MaxOutputTokensForConsistency
        }, cancellationToken);

        return new ItemConsistencyResult(
            MatchesSubcategory:       dto.MatchesSubcategory ?? false,
            Reason:                   dto.Reason,
            SuggestedSubcategoryCode: dto.SuggestedSubcategoryCode);
    }

    // ── Private DTOs ────────────────────────────────────────────────────────
    private sealed class PersonalBelongingDto
    {
        [JsonPropertyName("itemName")]         public string? ItemName { get; set; }
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
        [JsonPropertyName("itemName")]               public string? ItemName { get; set; }
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
        [JsonPropertyName("itemName")]      public string? ItemName { get; set; }
        [JsonPropertyName("primaryColor")]  public string? PrimaryColor { get; set; }
        [JsonPropertyName("aiDescription")] public string? AiDescription { get; set; }
    }

    private sealed class ConsistencyDto
    {
        [JsonPropertyName("matchesSubcategory")]       public bool? MatchesSubcategory { get; set; }
        [JsonPropertyName("suggestedSubcategoryCode")] public string? SuggestedSubcategoryCode { get; set; }
        [JsonPropertyName("reason")]                   public string? Reason { get; set; }
    }
}
