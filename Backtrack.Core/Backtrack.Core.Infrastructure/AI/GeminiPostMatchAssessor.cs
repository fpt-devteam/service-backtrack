using Backtrack.Core.Application.Interfaces.AI;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiPostMatchAssessor(ILlmService llmService) : IPostMatchAssessor
{
    private const string SystemPrompt = """
        You are a lost-and-found matching expert. You will receive the description of a lost item
        and a found item, along with comparison metrics (distance, time gap, similarity score, and matching level).

        Assess whether they are likely the same item based on the descriptions and metrics provided.

        Write a single "summary" sentence beginning with one of:
          "Unlikely match", "Possible match", "Likely match", or "Very likely match"
        followed by the primary reason.

        Respond ONLY with valid JSON — no markdown, no extra text:
        { "summary": "..." }
        """;

    public async Task<PostMatchAssessment> AssessAsync(
        PostMatchContext context,
        CancellationToken cancellationToken = default)
    {
        var dto = await llmService.CompleteAsync<AssessmentDto>(new LlmRequest
        {
            SystemPrompt    = SystemPrompt,
            UserPrompt      = BuildUserPrompt(context),
            Temperature     = 0.2f,
            MaxOutputTokens = 300
        }, cancellationToken);

        return new PostMatchAssessment { Summary = dto.Summary ?? string.Empty };
    }

    private static string BuildUserPrompt(PostMatchContext ctx) => $"""
        --- LOST ITEM ---
        {ctx.LostDescription}

        --- FOUND ITEM ---
        {ctx.FoundDescription}

        --- COMPARISON METRICS ---
        Distance:         {ctx.DistanceMeters:F0} metres
        Time gap:         {ctx.TimeGapDays:F1} days
        Similarity score: {ctx.MatchScore:P0}
        Matching level:   {ctx.MatchingLevel}

        Assess whether these two items are likely the same.
        """;

    private sealed class AssessmentDto
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
    }
}
