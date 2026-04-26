using System.Text.Json.Serialization;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiPostMatchAssessor(ILlmService llmService) : IPostMatchAssessor
{
    // ── Shared output format — appended to every category prompt ──────────────

    private const string OutputFormat = """

        After extracting evidence, make a final verdict:
          - is_match: true if the evidence suggests these are LIKELY the same physical item, false otherwise.
          - reasoning: one sentence explaining your verdict based on the evidence you found.

        Critical rules:
          - ONLY include attributes that are explicitly stated or clearly visible in BOTH posts.
          - If an attribute is mentioned in one post but absent in the other, SKIP it entirely.
          - Do NOT guess, infer, or fabricate any value.
          - ONE attribute per evidence entry. If a post mentions multiple distinct features
            (e.g. "quilted pattern" AND "gold clasp"), create SEPARATE entries for each.
            Never bundle multiple attributes into a single key.
          - strength meanings:
              "Strong"   = clearly the same (exact or synonymous, e.g. "black" vs "màu đen")
              "Partial"  = plausibly the same but described differently — two people can describe
                           the same physical detail using different words (e.g. "gold clasp" vs
                           "gold circle logo" could be the same hardware seen from different angles).
                           Use Partial when you cannot confirm OR deny they are the same.
              "Mismatch" = clearly contradictory and impossible to reconcile
                           (e.g. "red" vs "blue", "Nike" vs "Adidas")
          - is_match should be false ONLY when there is a clear, irreconcilable Mismatch on a
            primary attribute (color, brand, item type) or when there are zero Strong matches.
            Partial entries alone do NOT disqualify a match.

        Respond ONLY with valid JSON — no markdown, no extra text:
        {
          "is_match": true,
          "reasoning": "Both posts describe a black Herschel backpack with a cat sticker on the front.",
          "evidence": [
            {
              "key": "attribute name in snake_case",
              "lost_value": "what the lost post says",
              "found_value": "what the found post says",
              "strength": "Strong|Partial|Mismatch",
              "note": "one short phrase explaining your judgment, or null"
            }
          ]
        }
        """;

    // ── Category-specific system prompts ──────────────────────────────────────

    private static string PersonalBelongingsPrompt => $"""
        You are comparing two lost-and-found posts about a personal belonging.
        Your job: find concrete attribute matches between the lost item and the found item.

        Look for these attributes (skip any not mentioned in both posts):
          - brand
          - color (primary and secondary)
          - material (leather, fabric, plastic, metal…)
          - size or dimensions
          - distinctive_marks (scratches, stickers, engravings, stains, damage)
          - contents (what was inside, if mentioned)
          - any other concrete physical attribute both posts describe
        {OutputFormat}
        """;

    private static string ElectronicsPrompt => $"""
        You are comparing two lost-and-found posts about an electronic device.
        Your job: find concrete attribute matches between the lost item and the found item.

        Look for these attributes (skip any not mentioned in both posts):
          - brand
          - model
          - color
          - case_or_cover (phone case, laptop sleeve, protective cover)
          - lock_screen (wallpaper, displayed info)
          - distinguishing_features (scratches, stickers, cracks, dents)
          - accessories (charger, earbuds, stylus attached)
          - any other concrete physical attribute both posts describe
        {OutputFormat}
        """;

    private static string OthersPrompt => $"""
        You are comparing two lost-and-found posts about a miscellaneous item.
        Your job: find concrete attribute matches between the lost item and the found item.

        Look for these attributes (skip any not mentioned in both posts):
          - item_type (what kind of item)
          - color
          - size or shape
          - material
          - distinctive_marks (text printed, labels, logos, damage)
          - any other concrete physical attribute both posts describe
        {OutputFormat}
        """;

    // ── Entry point ───────────────────────────────────────────────────────────

    public async Task<PostMatchAssessment> AssessAsync(
        PostMatchContext context,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = context.Category switch
        {
            ItemCategory.PersonalBelongings => PersonalBelongingsPrompt,
            ItemCategory.Electronics => ElectronicsPrompt,
            _ => OthersPrompt,
        };

        var dto = await llmService.CompleteAsync<EvidenceDto>(new LlmRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = BuildUserPrompt(context),
            Temperature = 0.1f,
            MaxOutputTokens = 2500
        }, cancellationToken);

        var evidence = dto.Evidence?
            .Where(e => !string.IsNullOrWhiteSpace(e.Key)
                     && !string.IsNullOrWhiteSpace(e.LostValue)
                     && !string.IsNullOrWhiteSpace(e.FoundValue)
                     && Enum.TryParse<MatchStrength>(e.Strength, out _))
            .Select(e => new MatchEvidence(
                e.Key!,
                Enum.Parse<MatchStrength>(e.Strength!),
                e.LostValue!,
                e.FoundValue!,
                e.Note))
            .ToList() ?? [];

        return new PostMatchAssessment
        {
            IsMatch = dto.IsMatch,
            Reasoning = dto.Reasoning,
            Evidence = evidence,
        };
    }

    // ── User prompt — description only, no metrics ────────────────────────────

    private static string BuildUserPrompt(PostMatchContext ctx) => $"""
        --- LOST ITEM ---
        {ctx.LostDescription}

        --- FOUND ITEM ---
        {ctx.FoundDescription}

        Compare these two posts and extract matching evidence.
        """;

    // ── DTOs for JSON parsing ─────────────────────────────────────────────────

    private sealed class EvidenceDto
    {
        [JsonPropertyName("is_match")]
        public required bool IsMatch { get; set; }

        [JsonPropertyName("reasoning")]
        public required string Reasoning { get; set; }

        [JsonPropertyName("evidence")]
        public required List<EvidenceItemDto> Evidence { get; set; }
    }

    private sealed class EvidenceItemDto
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("strength")]
        public string? Strength { get; set; }

        [JsonPropertyName("lost_value")]
        public string? LostValue { get; set; }

        [JsonPropertyName("found_value")]
        public string? FoundValue { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}