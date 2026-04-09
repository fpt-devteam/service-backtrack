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
/// Cross-encoder reranker backed by Qwen3-Reranker-0.6B served locally via vLLM.
///
/// Uses POST /v1/score (NOT /v1/rerank).
///   /v1/rerank  → softmax-normalised across the batch  → all scores ~1/N, useless for ranking
///   /v1/score   → independent sigmoid per document     → true relevance scores in [0, 1]
/// </summary>
public sealed class QwenCrossEncoderService : ICrossEncoderService
{
    private readonly HttpClient _httpClient;
    private readonly QwenRerankerSettings _settings;
    private readonly ILogger<QwenCrossEncoderService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public QwenCrossEncoderService(
        HttpClient httpClient,
        IOptions<QwenRerankerSettings> settings,
        ILogger<QwenCrossEncoderService> logger)
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

        // Qwen3-Reranker expects the instruction prepended to the query.
        // This guides the model to judge lost-and-found relevance specifically.
        var formattedQuery = $"Instruct: Given a lost-and-found search query, retrieve relevant lost or found item descriptions\nQuery: {query}";

        var request = new ScoreRequest
        {
            Model  = _settings.ModelName,
            Text1  = formattedQuery,
            Text2  = documents.ToList(),
        };

        var url  = $"{_settings.BaseUrl.TrimEnd('/')}/v1/score";
        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(url, content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Qwen reranker unreachable at {Url}", url);
            throw new InvalidOperationException($"Qwen reranker is unreachable at {url}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Qwen reranker returned {Status}: {Error}", (int)response.StatusCode, error);
            throw new InvalidOperationException($"Qwen reranker returned {(int)response.StatusCode}: {error}");
        }

        ScoreResponse? result;
        try
        {
            result = await response.Content.ReadFromJsonAsync<ScoreResponse>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize Qwen score response");
            throw new InvalidOperationException("Failed to deserialize Qwen reranker response", ex);
        }

        if (result?.Data is not { Count: > 0 })
        {
            _logger.LogWarning("Qwen reranker returned empty data");
            throw new InvalidOperationException("Qwen reranker returned empty results");
        }

        // vLLM /v1/score returns results keyed by original index — restore original order
        var scores = new double[documents.Count];
        foreach (var item in result.Data)
        {
            if (item.Index >= 0 && item.Index < documents.Count)
                scores[item.Index] = item.Score;
        }

        return scores;
    }

    #region Request / Response DTOs

    private sealed class ScoreRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>Single query string (broadcast against all documents in Text2).</summary>
        [JsonPropertyName("text_1")]
        public string Text1 { get; set; } = string.Empty;

        /// <summary>List of document strings to score against the query.</summary>
        [JsonPropertyName("text_2")]
        public List<string> Text2 { get; set; } = [];
    }

    private sealed class ScoreResponse
    {
        [JsonPropertyName("data")]
        public List<ScoreItem>? Data { get; set; }
    }

    private sealed class ScoreItem
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }

    #endregion
}
