using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiPostMatchAssessor : IPostMatchAssessor
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;

    private const string ModelName = "gemini-2.0-flash";

    private const string SystemPrompt = """
        You are a lost-and-found matching expert. You will receive the description of a lost item and a found item, along with comparison metrics (distance, time gap, similarity score, and matching level).

        Assess whether they are likely the same item based on the descriptions and metrics provided.

        Write a single "summary" sentence beginning with one of:
          "Unlikely match", "Possible match", "Likely match", or "Very likely match"
        followed by the primary reason.

        Respond ONLY with valid JSON — no markdown, no extra text:
        {
            "summary": "..."
        }
        """;

    public GeminiPostMatchAssessor(HttpClient httpClient, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient;
        _settings   = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");
    }

    public async Task<PostMatchAssessment> AssessAsync(
        PostMatchContext context,
        CancellationToken cancellationToken = default)
    {
        var userPrompt = BuildUserPrompt(context);
        var request    = BuildRequest(userPrompt);

        var url      = $"{_settings.BaseUrl}/{ModelName}:generateContent?key={_settings.ApiKey}";
        var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);

        var responseText = result?.Candidates?[0].Content?.Parts?[0].Text
            ?? throw new InvalidOperationException("Empty response from Gemini LLM API.");

        var dto = GeminiResponseParser.Parse<AssessmentDto>(responseText);

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

    private static GeminiRequest BuildRequest(string userPrompt) => new()
    {
        SystemInstruction = new GeminiContent { Parts = [new GeminiPart { Text = SystemPrompt }] },
        Contents          = [new GeminiContent { Parts = [new GeminiPart { Text = userPrompt }] }],
        GenerationConfig  = new GeminiGenerationConfig { Temperature = 0.2f, MaxOutputTokens = 300 }
    };

    #region Request / Response DTOs

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

    private sealed class AssessmentDto
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
    }

    #endregion
}
