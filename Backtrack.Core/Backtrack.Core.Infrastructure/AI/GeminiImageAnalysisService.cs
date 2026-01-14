using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

/// <summary>
/// Implementation of image analysis service using Google's Gemini Vision API.
/// Analyzes images to extract item information for lost and found posts.
/// </summary>
public sealed class GeminiImageAnalysisService : IImageAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;

    private const string VisionModelName = "gemini-2.0-flash";

    private const string AnalysisPrompt = """
        You are an AI assistant specialized in analyzing images of lost or found items.
        Analyze the image and extract item information for a lost and found platform.

        Respond in JSON format:
        {
            "itemName": "Concise item name (3-6 words, e.g., 'Black Leather Wallet', 'Silver iPhone 15 Pro')",
            "description": "Structured description with each attribute on a new line"
        }

        Description format (each on new line, omit if not visible):
        - Category: [Electronics/Clothing/Accessories/Documents/Bags/Keys/etc.]
        - Color: [Primary color, secondary if applicable]
        - Brand: [Brand name if visible]
        - Condition: [New/Used/Worn/Damaged]
        - Material: [Leather/Fabric/Metal/Plastic/etc.]
        - Size: [Small/Medium/Large or dimensions]
        - Distinctive marks: [Unique features, scratches, stickers, patterns]

        Example description:
        "- Category: Electronics\n- Color: Space Gray\n- Brand: Apple\n- Condition: Used\n- Material: Aluminum and glass\n- Distinctive marks: Small scratch on back, red case"

        Guidelines:
        1. Only describe what you can actually see - omit unknown attributes
        2. Keep each line short and meaningful
        3. If image is unclear, set itemName to "Unidentifiable Item"

        Respond ONLY with the JSON object, no markdown.
        """;

    public GeminiImageAnalysisService(HttpClient httpClient, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Gemini API key is not configured. Please set 'GeminiSettings__ApiKey' in configuration.");
    }

    public async Task<ImageAnalysisOutput> AnalyzeImageAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageBase64))
            throw new ArgumentException("Image data cannot be null or empty.", nameof(imageBase64));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type cannot be null or empty.", nameof(mimeType));

        var url = $"{_settings.BaseUrl}/{VisionModelName}:generateContent?key={_settings.ApiKey}";

        var request = new GeminiVisionRequest
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new GeminiPart
                        {
                            Text = AnalysisPrompt
                        },
                        new GeminiPart
                        {
                            InlineData = new GeminiInlineData
                            {
                                MimeType = mimeType,
                                Data = imageBase64
                            }
                        }
                    }
                }
            },
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.2f,
                MaxOutputTokens = 1024
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiVisionResponse>(cancellationToken);

        if (result?.Candidates == null || result.Candidates.Count == 0)
            throw new InvalidOperationException("No response returned from Gemini Vision API.");

        var responseText = result.Candidates[0].Content?.Parts?[0]?.Text;
        if (string.IsNullOrWhiteSpace(responseText))
            throw new InvalidOperationException("Empty response text from Gemini Vision API.");

        // Parse the JSON response
        var analysisResult = ParseAnalysisResponse(responseText);

        return new ImageAnalysisOutput
        {
            ItemName = analysisResult.ItemName,
            Description = analysisResult.Description
        };
    }

    private static ImageAnalysisJsonResponse ParseAnalysisResponse(string responseText)
    {
        // Clean up the response - remove markdown code blocks if present
        var cleanedResponse = responseText.Trim();
        if (cleanedResponse.StartsWith("```json"))
            cleanedResponse = cleanedResponse[7..];
        else if (cleanedResponse.StartsWith("```"))
            cleanedResponse = cleanedResponse[3..];

        if (cleanedResponse.EndsWith("```"))
            cleanedResponse = cleanedResponse[..^3];

        cleanedResponse = cleanedResponse.Trim();

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<ImageAnalysisJsonResponse>(cleanedResponse, options);

            if (result == null)
                throw new InvalidOperationException("Failed to parse image analysis response.");

            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse Gemini response as JSON: {ex.Message}. Response: {cleanedResponse}");
        }
    }

    #region Request/Response DTOs

    private class GeminiVisionRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("inlineData")]
        public GeminiInlineData? InlineData { get; set; }
    }

    private class GeminiInlineData
    {
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
    }

    private class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }
    }

    private class GeminiVisionResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private class ImageAnalysisJsonResponse
    {
        [JsonPropertyName("itemName")]
        public string ItemName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    #endregion
}
