using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.WebApi.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI
{
    /// <summary>
    /// Implementation of embedding service using Google's Gemini API.
    /// Uses the text-embedding-004 model for generating embeddings.
    /// </summary>
    public sealed class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;

        public int EmbeddingDimension => 768; // Gemini text-embedding-004 produces 768-dimensional vectors

        public GeminiEmbeddingService(HttpClient httpClient, IOptions<GeminiSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException("Gemini API key is not configured. Please set 'GeminiSettings__ApiKey' in configuration.");
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));

            var embeddings = await GenerateEmbeddingsBatchAsync(new[] { text }, cancellationToken);
            return embeddings.FirstOrDefault() ?? Array.Empty<float>();
        }

        public async Task<IList<float[]>> GenerateEmbeddingsBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var textList = texts?.ToList() ?? throw new ArgumentNullException(nameof(texts));

            if (textList.Count == 0)
                return Array.Empty<float[]>();

            var url = $"{_settings.BaseUrl}/{_settings.ModelName}:batchEmbedContents?key={_settings.ApiKey}";

            var request = new
            {
                requests = textList.Select(text => new
                {
                    model = $"models/{_settings.ModelName}",
                    content = new
                    {
                        parts = new[] { new { text } }
                    }
                }).ToList()
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GeminiBatchEmbeddingResponse>(cancellationToken);

                if (result?.Embeddings == null || result.Embeddings.Count == 0)
                    throw new InvalidOperationException("No embeddings returned from Gemini API.");

                return result.Embeddings.Select(e => e.Values).ToList();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to generate embeddings from Gemini API: {ex.Message}", ex);
            }
        }

        // Response DTOs for Gemini API
        private class GeminiBatchEmbeddingResponse
        {
            [JsonPropertyName("embeddings")]
            public List<EmbeddingData> Embeddings { get; set; } = new();
        }

        private class EmbeddingData
        {
            [JsonPropertyName("values")]
            public float[] Values { get; set; } = Array.Empty<float>();
        }
    }
}
