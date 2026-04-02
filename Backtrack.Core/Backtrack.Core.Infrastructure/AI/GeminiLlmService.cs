using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Domain.ValueObjects;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IImageFetcher _imageFetcher;
    private readonly GeminiSettings _settings;

    private const string ModelName = "gemini-2.0-flash";

    private const string SystemPrompt = """
        You are a lost-and-found matching expert. Pre-computed similarity scores have already been calculated for each criterion using embedding models and deterministic formulas. Your task is to provide reasoning and justification that explains why these scores are appropriate, by examining the actual post details and images provided.

        For each criterion, provide 2 to 4 labelled reasoning points. Each point has:
          • "label": a short title (3–5 words, e.g. "Color Match", "Same Neighborhood", "Date Gap Plausible")
          • "detail": one clear, specific sentence explaining the supporting or contradicting evidence

        The four criteria are:

        1. visualAnalysis  [HIGHEST WEIGHT]
           Examine images closely. Compare color, shape, size, texture, brand markings, logos, wear patterns, distinctive physical features. If images are provided, reference them directly. If not, reason from visual descriptions in the text.
           The provided VisualScore reflects image embedding cosine similarity — justify why that score is appropriate.

        2. description  [HIGH WEIGHT]
           Compare the written descriptions: item name/type, stated material, brand, model, size, condition, and distinguishing features. The DescriptionScore reflects text embedding similarity — justify it.

        3. location  [MODERATE WEIGHT]
           Assess geographic plausibility using the actual distance and display addresses. The LocationScore reflects proximity — justify it by reasoning about neighborhood-level and venue-level match. Include the actual distance figure.

        4. timeWindow  [MODERATE WEIGHT]
           Assess temporal plausibility: is the time gap between the loss event and the find realistic? The TimeWindowScore reflects the event time delta — justify it by referencing the actual dates.

        For each criterion: if the score is high (≥70), highlight what matches well. If the score is low (<50), highlight what does not match or is uncertain.

        At the end, write a single "summary" sentence synthesising all criteria. Begin with one of:
          "Unlikely match", "Possible match", "Likely match", or "Very likely match"
        followed by the primary reason.

        Respond ONLY with valid JSON — no markdown, no extra text:
        {
            "criteria": {
                "visualAnalysis": {
                    "points": [
                        { "label": "...", "detail": "..." },
                        ...
                    ]
                },
                "description": {
                    "points": [ ... ]
                },
                "location": {
                    "points": [ ... ]
                },
                "timeWindow": {
                    "points": [ ... ]
                }
            },
            "summary": "..."
        }
        """;

    public GeminiLlmService(HttpClient httpClient, IImageFetcher imageFetcher, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient;
        _imageFetcher = imageFetcher;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");
    }

    public async Task<PostMatchAssessment> AssessPostMatchAsync(
        PostMatchContext lostPost,
        PostMatchContext foundPost,
        PostMatchScores scores,
        float distanceMeters,
        CancellationToken cancellationToken = default)
    {
        var lostImage = await _imageFetcher.FetchAsync(lostPost.ImageUrl, cancellationToken);
        var foundImage = await _imageFetcher.FetchAsync(foundPost.ImageUrl, cancellationToken);

        var userPrompt = BuildUserPrompt(lostPost, foundPost, scores, distanceMeters);
        var request = BuildRequest(userPrompt, lostImage, foundImage);

        var url = $"{_settings.BaseUrl}/{ModelName}:generateContent?key={_settings.ApiKey}";
        var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);

        var responseText = result?.Candidates?[0].Content?.Parts?[0].Text
            ?? throw new InvalidOperationException("Empty response from Gemini LLM API.");

        var dto = GeminiResponseParser.Parse<AssessmentDto>(responseText);

        return new PostMatchAssessment
        {
            Criteria = new PostMatchCriteriaAssessment
            {
                VisualAnalysis = MapPoints(dto.Criteria?.VisualAnalysis),
                Description = MapPoints(dto.Criteria?.Description),
                Location = MapPoints(dto.Criteria?.Location),
                TimeWindow = MapPoints(dto.Criteria?.TimeWindow)
            },
            Summary = dto.Summary ?? string.Empty
        };
    }

    private static string BuildUserPrompt(
        PostMatchContext lostPost,
        PostMatchContext foundPost,
        PostMatchScores scores,
        float distanceMeters)
    {
        return $"""
            --- LOST ITEM ---
            Title: {lostPost.ItemName}
            Description: {lostPost.Description}
            Event time: {lostPost.EventTime:yyyy-MM-dd HH:mm} UTC
            Location: {lostPost.DisplayAddress ?? "not specified"}

            --- FOUND ITEM ---
            Title: {foundPost.ItemName}
            Description: {foundPost.Description}
            Event time: {foundPost.EventTime:yyyy-MM-dd HH:mm} UTC
            Location: {foundPost.DisplayAddress ?? "not specified"}

            --- PRE-COMPUTED SCORES (justify these with your reasoning) ---
            Description (text embedding):   {scores.DescriptionScore}/100
            Visual (image embedding):       {scores.VisualScore}/100
            Location (proximity formula):   {scores.LocationScore}/100  [{distanceMeters:F0} metres apart]
            Time window (time-gap formula): {scores.TimeWindowScore}/100

            Provide reasoning points that justify each of these scores based on the post details and images above.
            """;
    }

    private static GeminiRequest BuildRequest(
        string userPrompt,
        FetchedImage? lostImage,
        FetchedImage? foundImage)
    {
        var parts = new List<GeminiPart> { new() { Text = userPrompt } };

        if (lostImage is not null)
        {
            parts.Add(new GeminiPart { Text = "Image of the LOST item:" });
            parts.Add(new GeminiPart
            {
                InlineData = new GeminiInlineData
                {
                    MimeType = lostImage.MimeType,
                    Data = lostImage.Base64
                }
            });
        }

        if (foundImage is not null)
        {
            parts.Add(new GeminiPart { Text = "Image of the FOUND item:" });
            parts.Add(new GeminiPart
            {
                InlineData = new GeminiInlineData
                {
                    MimeType = foundImage.MimeType,
                    Data = foundImage.Base64
                }
            });
        }

        return new GeminiRequest
        {
            SystemInstruction = new GeminiContent { Parts = [new GeminiPart { Text = SystemPrompt }] },
            Contents = [new GeminiContent { Parts = parts }],
            GenerationConfig = new GeminiGenerationConfig { Temperature = 0.2f, MaxOutputTokens = 1500 }
        };
    }

    private static CriterionPoint[] MapPoints(CriterionDto? dto)
    {
        return dto?.Points?.Select(p => new CriterionPoint
        {
            Label = p.Label ?? string.Empty,
            Detail = p.Detail ?? string.Empty
        }).ToArray() ?? [];
    }

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

        [JsonPropertyName("inlineData")]
        public GeminiInlineData? InlineData { get; set; }
    }

    private sealed class GeminiInlineData
    {
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
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

    // --- LLM response shape ---

    private sealed class AssessmentDto
    {
        [JsonPropertyName("criteria")]
        public CriteriaDto? Criteria { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
    }

    private sealed class CriteriaDto
    {
        [JsonPropertyName("visualAnalysis")]
        public CriterionDto? VisualAnalysis { get; set; }

        [JsonPropertyName("description")]
        public CriterionDto? Description { get; set; }

        [JsonPropertyName("location")]
        public CriterionDto? Location { get; set; }

        [JsonPropertyName("timeWindow")]
        public CriterionDto? TimeWindow { get; set; }
    }

    private sealed class CriterionDto
    {
        [JsonPropertyName("points")]
        public List<PointDto>? Points { get; set; }
    }

    private sealed class PointDto
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("detail")]
        public string? Detail { get; set; }
    }

    #endregion
}
