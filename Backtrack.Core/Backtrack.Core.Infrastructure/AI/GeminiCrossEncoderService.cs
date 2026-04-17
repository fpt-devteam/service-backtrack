using Backtrack.Core.Application.Interfaces.AI;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiCrossEncoderService(
    ILlmService llmService,
    ILogger<GeminiCrossEncoderService> logger) : ICrossEncoderService
{
    private const string SystemPrompt = """
        You are a relevance scoring engine for a lost-and-found search platform.
        Given a user query and a numbered list of item descriptions, score each item's relevance to the query.

        Scoring criteria:
        - Item type / name match (most important)
        - Key attributes: color, brand, size, material, condition, distinctive marks
        - Any detail overlap between query and item description

        Return ONLY valid JSON with a "scores" array of floats in [0, 1], one per item,
        in the exact same order as the input. No markdown, no explanation.
        Example response: {"scores": [0.95, 0.3, 0.72]}
        """;

    public async Task<double[]> ScoreAsync(
        string query,
        IReadOnlyList<string> documents,
        CancellationToken cancellationToken = default)
    {
        if (documents.Count == 0)
            return [];

        try
        {
            var dto = await llmService.CompleteAsync<ScoresDto>(new LlmRequest
            {
                SystemPrompt    = SystemPrompt,
                UserPrompt      = BuildPrompt(query, documents),
                Temperature     = 0.0f,
                MaxOutputTokens = 512
            }, cancellationToken);

            if (dto.Scores is { Length: > 0 })
            {
                var scores = dto.Scores;
                if (scores.Length < documents.Count)
                    scores = [.. scores, .. Enumerable.Repeat(0.0, documents.Count - scores.Length)];
                return scores[..documents.Count];
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cross-encoder scoring failed, returning uniform scores.");
        }

        return Enumerable.Repeat(0.5, documents.Count).ToArray();
    }

    private static string BuildPrompt(string query, IReadOnlyList<string> documents)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Query: \"{query}\"");
        sb.AppendLine();
        sb.AppendLine("Items to score:");
        for (int i = 0; i < documents.Count; i++)
            sb.AppendLine($"[{i}]: {documents[i]}");
        return sb.ToString();
    }

    private sealed class ScoresDto
    {
        [JsonPropertyName("scores")]
        public double[]? Scores { get; set; }
    }
}
