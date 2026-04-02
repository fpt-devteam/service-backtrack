using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
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
            "category": "One of: Electronics, Clothing, Accessories, Documents, Bags, Keys, Other",
            "color": "Primary color, secondary if applicable",
            "brand": "Brand name if visible",
            "condition": "New/Used/Worn/Damaged",
            "material": "Leather/Fabric/Metal/Plastic/etc.",
            "size": "Small/Medium/Large or dimensions",
            "distinctiveMarks": "Unique features, scratches, stickers, patterns",
            "additionalDetails": "Any other relevant info that doesn't fit in other fields"
        }

        Guidelines:
        1. Only describe what you can actually see - omit unknown attributes or set them to null.
        2. Keep each field short and meaningful.
        3. For 'category', you MUST use one of these exact values: Electronics, Clothing, Accessories, Documents, Bags, Keys, Other.
        4. If image is unclear, set itemName to "Unidentifiable Item".

        Respond ONLY with the JSON object, no markdown.
        """;

    public GeminiImageAnalysisService(HttpClient httpClient, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Gemini API key is not configured. Please set 'GeminiSettings__ApiKey' in configuration.");
    }

    public async Task<PostItem> AnalyzeImageAsync(
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
        var analysisResult = GeminiResponseParser.Parse<ImageAnalysisJsonResponse>(responseText);

        return new PostItem
        {
            ItemName = analysisResult.ItemName,
            Category = analysisResult.Category ?? ItemCategory.Other,
            Color = analysisResult.Color,
            Brand = analysisResult.Brand,
            Condition = analysisResult.Condition,
            Material = analysisResult.Material,
            Size = analysisResult.Size,
            DistinctiveMarks = analysisResult.DistinctiveMarks,
            AdditionalDetails = analysisResult.AdditionalDetails
        };
    }

    #region Request/Response DTOs

    private sealed class GeminiVisionRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
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

    private sealed class GeminiVisionResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class ImageAnalysisJsonResponse
    {
        [JsonPropertyName("itemName")]
        public string ItemName { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public ItemCategory? Category { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("material")]
        public string? Material { get; set; }

        [JsonPropertyName("size")]
        public string? Size { get; set; }

        [JsonPropertyName("distinctiveMarks")]
        public string? DistinctiveMarks { get; set; }

        [JsonPropertyName("additionalDetails")]
        public string? AdditionalDetails { get; set; }
    }

    #endregion
}
