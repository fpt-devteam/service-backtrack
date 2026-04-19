using System.Text.Json.Serialization;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiPostMatchAssessor(ILlmService llmService) : IPostMatchAssessor
{
    // ── Shared instructions appended to every category prompt ─────────────────

    private const string SharedInstructions = """

        After evaluating item-specific fields, always include two additional evidence entries:
          - key "location": compare the distance between where the item was lost and found.
          - key "time_gap": compare how many days apart the two events occurred.

        Rules:
          - strength must be one of: Strong, Partial, Weak, Mismatch
          - display_value: a short human-readable value (e.g. "NGÔ ĐỨC BÌNH", "1.2 km apart", "1 day apart")
          - note: one short explanatory phrase, or null if self-evident
          - Omit keys that have no information in either post (do not invent data)

        Respond ONLY with valid JSON — no markdown, no extra text:
        {
          "evidence": [
            { "key": "...", "strength": "Strong|Partial|Weak|Mismatch", "display_value": "...", "note": "..." }
          ]
        }
        """;

    // ── Category-specific system prompts ──────────────────────────────────────

    private static string CardPrompt => $"""
        You are a lost-and-found matching expert specialising in identity and membership cards.

        You will receive the description of a lost card and a found card.
        Evaluate the following evidence keys IN ORDER OF IMPORTANCE:
          1. "holder_name"      — does the cardholder name match? (exact = Strong, similar = Partial, different = Mismatch)
          2. "issuing_authority"— same institution/organisation?
          3. "card_type"        — same type of card (student, national ID, bank card…)?
          4. "expiry_date"      — do expiry dates align?
          5. "ocr_text"         — do other details from the scanned text corroborate the match?
        {SharedInstructions}
        """;

    private static string PersonalBelongingsPrompt => $"""
        You are a lost-and-found matching expert specialising in personal belongings.

        You will receive the description of a lost item and a found item.
        Evaluate the following evidence keys IN ORDER OF IMPORTANCE:
          1. "brand"            — same brand?
          2. "color"            — same primary colour?
          3. "material"         — same material?
          4. "distinctive_marks"— unique marks, engravings, stickers, damage?
          5. "condition"        — reported condition consistent?
          6. "description"      — do the overall descriptions align?
        {SharedInstructions}
        """;

    private static string ElectronicsPrompt => $"""
        You are a lost-and-found matching expert specialising in electronic devices.

        You will receive the description of a lost device and a found device.
        Evaluate the following evidence keys IN ORDER OF IMPORTANCE:
          1. "brand"                  — same brand?
          2. "model"                  — same model?
          3. "color"                  — same colour?
          4. "distinguishing_features"— unique scratches, stickers, cases, accessories?
          5. "lock_screen"            — lock screen description consistent?
          6. "description"            — overall description alignment?
        {SharedInstructions}
        """;

    private static string OthersPrompt => $"""
        You are a lost-and-found matching expert.

        You will receive the description of a lost item and a found item.
        Evaluate the following evidence keys IN ORDER OF IMPORTANCE:
          1. "item_type"  — same type of item?
          2. "color"      — same primary colour?
          3. "description"— do the overall descriptions align (size, shape, markings)?
        {SharedInstructions}
        """;

    // ── Entry point ───────────────────────────────────────────────────────────

    public async Task<PostMatchAssessment> AssessAsync(
        PostMatchContext context,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = context.Category switch
        {
            ItemCategory.Cards              => CardPrompt,
            ItemCategory.PersonalBelongings => PersonalBelongingsPrompt,
            ItemCategory.Electronics        => ElectronicsPrompt,
            _                               => OthersPrompt,
        };

        var dto = await llmService.CompleteAsync<EvidenceDto>(new LlmRequest
        {
            SystemPrompt    = systemPrompt,
            UserPrompt      = BuildUserPrompt(context),
            Temperature     = 0.1f,
            MaxOutputTokens = 2000
        }, cancellationToken);

        var evidence = dto.Evidence?
            .Where(e => !string.IsNullOrWhiteSpace(e.Key) && Enum.TryParse<MatchStrength>(e.Strength, out _))
            .Select(e => new MatchEvidence(
                e.Key!,
                Enum.Parse<MatchStrength>(e.Strength!),
                e.DisplayValue,
                e.Note))
            .ToList() ?? [];

        return new PostMatchAssessment { Evidence = evidence };
    }

    // ── User prompt (same structure for all categories) ───────────────────────

    private static string BuildUserPrompt(PostMatchContext ctx) => $"""
        --- LOST ITEM ---
        {ctx.LostDescription}

        --- FOUND ITEM ---
        {ctx.FoundDescription}

        --- CONTEXT METRICS ---
        Distance:         {ctx.DistanceMeters:F0} metres
        Time gap:         {ctx.TimeGapDays:F1} days
        Similarity score: {ctx.MatchScore:P0}
        Matching level:   {ctx.MatchingLevel}

        Assess whether these two items are likely the same, returning structured evidence.
        """;

    // ── DTOs for JSON parsing ─────────────────────────────────────────────────

    private sealed class EvidenceDto
    {
        [JsonPropertyName("evidence")]
        public List<EvidenceItemDto>? Evidence { get; set; }
    }

    private sealed class EvidenceItemDto
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("strength")]
        public string? Strength { get; set; }

        [JsonPropertyName("display_value")]
        public string? DisplayValue { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}
