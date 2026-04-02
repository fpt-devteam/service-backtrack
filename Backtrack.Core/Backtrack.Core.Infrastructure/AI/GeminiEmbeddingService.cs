using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI
{
    public sealed class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiEmbeddingService> _logger;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public int EmbeddingDimension => _settings.EmbeddingDimension;

        public GeminiEmbeddingService(HttpClient httpClient, IOptions<GeminiSettings> settings, ILogger<GeminiEmbeddingService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException("Gemini API key is not configured. Please set 'GeminiSettings__ApiKey' in configuration.");
        }

        #region Text Embedding
        public async Task<float[]> GenerateTextEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<float>();

            var url = $"{_settings.BaseUrl}/{_settings.ModelName}:embedContent?key={_settings.ApiKey}";

            var request = new EmbedContentRequest
            {
                Content = new EmbedContent { Parts = [new EmbedPart { Text = text }] },
                TaskType = "SEMANTIC_SIMILARITY",
                OutputDimensionality = EmbeddingDimension
            };

            var json = JsonSerializer.Serialize(request, SerializerOptions);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini embedContent failed. Response: {ErrorBody}", errorBody);
                throw new HttpRequestException(
                    $"Gemini embedContent failed ({(int)response.StatusCode} {response.ReasonPhrase}): {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>(cancellationToken);
            var values = result?.Embedding?.Values;
            if (values == null || values.Length == 0)
                throw new InvalidOperationException("Failed to parse response from Gemini API.");

            return values;
        }
        #endregion

        #region Multimodal Embedding
        public async Task<float[]> GenerateMultimodalEmbeddingAsync(
            string? text = null,
            string? imageBase64 = null,
            string? mimeType = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(imageBase64))
                throw new ArgumentException("At least text or image must be provided.");

            var url = $"{_settings.BaseUrl}/{_settings.ModelName}:embedContent?key={_settings.ApiKey}";

            var parts = new List<EmbedPart>();

            if (!string.IsNullOrWhiteSpace(text))
                parts.Add(new EmbedPart { Text = text });

            if (!string.IsNullOrWhiteSpace(imageBase64) && !string.IsNullOrWhiteSpace(mimeType))
                parts.Add(new EmbedPart { InlineData = new InlineData { MimeType = mimeType, Data = imageBase64 } });

            var request = new EmbedContentRequest
            {
                Content = new EmbedContent { Parts = parts },
                OutputDimensionality = EmbeddingDimension
            };

            var json = JsonSerializer.Serialize(request, SerializerOptions);
            _logger.LogWarning("Gemini embedContent request body: {RequestBody}", json);

            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini embedContent failed. URL: {Url} | Body sent: {RequestBody} | Response: {ErrorBody}",
                    $"...embedContent?key=***", json, errorBody);
                throw new HttpRequestException(
                    $"Gemini embedContent failed ({(int)response.StatusCode} {response.ReasonPhrase}): {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>(cancellationToken: cancellationToken);
            var values = result?.Embedding?.Values;
            if (values == null)
                throw new InvalidOperationException("No embedding returned from Gemini API.");

            return values;
        }
        #endregion

        #region Request DTOs

        private class EmbedContentRequest
        {
            [JsonPropertyName("content")]
            public EmbedContent Content { get; set; } = new();

            [JsonPropertyName("taskType")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? TaskType { get; set; }

            [JsonPropertyName("outputDimensionality")]
            public int? OutputDimensionality { get; set; }
        }

        private class EmbedContent
        {
            [JsonPropertyName("parts")]
            public List<EmbedPart> Parts { get; set; } = new();
        }

        private class EmbedPart
        {
            // Text part — omitted when null so image-only requests stay clean
            [JsonPropertyName("text")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Text { get; set; }

            // Image part — field name must be snake_case per Gemini REST API spec
            [JsonPropertyName("inline_data")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public InlineData? InlineData { get; set; }
        }

        private class InlineData
        {
            // snake_case required by Gemini REST API
            [JsonPropertyName("mime_type")]
            public string MimeType { get; set; } = string.Empty;

            [JsonPropertyName("data")]
            public string Data { get; set; } = string.Empty;
        }

        #endregion

        #region Response DTOs

        private sealed class GeminiEmbeddingResponse
        {
            [JsonPropertyName("embedding")]
            public EmbeddingData Embedding { get; set; } = new();
        }

        private sealed class EmbeddingData
        {
            [JsonPropertyName("values")]
            public float[] Values { get; set; } = Array.Empty<float>();
        }

        #endregion
    }
}
