using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

/// <summary>
/// Cross-encoder reranker implemented via Gemini Flash.
/// All documents are scored in a single API call.
/// </summary>
public sealed class GeminiCrossEncoderService : ICrossEncoderService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiCrossEncoderService> _logger;

    private const string RerankModel = "gemini-2.0-flash";

    private const string SystemPrompt = """
        You are a relevance scoring engine for a lost-and-found search platform.
        Given a user query and a numbered list of item descriptions, score each item's
        relevance to the query.

        Scoring criteria:
        - Item type / name match (most important)
        - Key attributes: color, brand, size, material, condition, distinctive marks
        - Any detail overlap between query and item description

        Return ONLY valid JSON with a "scores" array of floats in [0, 1], one per item,
        in the exact same order as the input. No markdown, no explanation.

        Example response: {"scores": [0.95, 0.3, 0.72]}
        """;

    public GeminiCrossEncoderService(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiCrossEncoderService> logger)
    {
        _httpClient = httpClient;
        _settings   = settings.Value;
        _logger     = logger;
    }

    public async Task<double[]> ScoreAsync(
        string query,
        IReadOnlyList<string> documents,
        CancellationToken cancellationToken = default)
    {
        if (documents.Count == 0)
            return [];

        var userPrompt = BuildPrompt(query, documents);
        var request    = new GeminiRequest
        {
            SystemInstruction = new GeminiContent { Parts = [new GeminiPart { Text = SystemPrompt }] },
            Contents          = [new GeminiContent { Parts = [new GeminiPart { Text = userPrompt }] }],
            GenerationConfig  = new GeminiGenerationConfig { Temperature = 0.0f, MaxOutputTokens = 512 }
        };

        var url      = $"{_settings.BaseUrl}/{RerankModel}:generateContent?key={_settings.ApiKey}";
        var json     = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gemini cross-encoder failed: {Error}", error);
            // Fall back: return uniform scores so ranking degrades gracefully
            return Enumerable.Repeat(0.5, documents.Count).ToArray();
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(
            cancellationToken: cancellationToken);

        var responseText = result?.Candidates?[0].Content?.Parts?[0].Text;
        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogWarning("Empty cross-encoder response from Gemini");
            return Enumerable.Repeat(0.5, documents.Count).ToArray();
        }

        try
        {
            var cleaned = GeminiResponseParser.CleanResponse(responseText);
            var dto     = JsonSerializer.Deserialize<ScoresDto>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto?.Scores is { Length: > 0 })
            {
                // Pad or trim to match input count defensively
                var scores = dto.Scores;
                if (scores.Length < documents.Count)
                    scores = [.. scores, .. Enumerable.Repeat(0.0, documents.Count - scores.Length)];
                return scores[..documents.Count];
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse cross-encoder scores, raw: {Raw}", responseText);
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

    #region DTOs

    private sealed class GeminiRequest
    {
        [JsonPropertyName("systemInstruction")]
        public GeminiContent? SystemInstruction { get; set; }

        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = [];

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class ScoresDto
    {
        [JsonPropertyName("scores")]
        public double[]? Scores { get; set; }
    }

    #endregion
}
