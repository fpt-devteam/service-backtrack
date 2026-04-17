using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiLlmService(
    HttpClient httpClient,
    IOptions<GeminiSettings> settings,
    ILogger<GeminiLlmService> logger) : ILlmService
{
    private readonly GeminiSettings _settings = settings.Value;

    public async Task<T> CompleteAsync<T>(LlmRequest request, CancellationToken cancellationToken = default) where T : class
    {
        var model = string.IsNullOrWhiteSpace(request.Model)
            ? _settings.GenerationModelName
            : request.Model;

        var url = $"{_settings.BaseUrl}/{model}:generateContent?key={_settings.ApiKey}";

        var parts = new List<GeminiGeneratePart>
        {
            new() { Text = request.UserPrompt }
        };

        if (request.Images is { Count: > 0 })
        {
            foreach (var img in request.Images)
            {
                parts.Add(new GeminiGeneratePart
                {
                    InlineData = new GeminiInlineData { MimeType = img.MimeType, Data = img.Base64 }
                });
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.ImageBase64) && !string.IsNullOrWhiteSpace(request.ImageMimeType))
        {
            parts.Add(new GeminiGeneratePart
            {
                InlineData = new GeminiInlineData
                {
                    MimeType = request.ImageMimeType,
                    Data = request.ImageBase64
                }
            });
        }

        var geminiRequest = new GeminiGenerateRequest
        {
            SystemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null
                : new GeminiGenerateContent { Parts = [new GeminiGeneratePart { Text = request.SystemPrompt }] },
            Contents = [new GeminiGenerateContent { Parts = parts }],
            GenerationConfig = new GeminiGenerateConfig
            {
                Temperature = request.Temperature,
                MaxOutputTokens = request.MaxOutputTokens
            }
        };

        var response = await httpClient.PostAsJsonAsync(url, geminiRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Gemini generateContent failed for model {Model}: {Error}", model, error);
            response.EnsureSuccessStatusCode(); // throws HttpRequestException
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiGenerateResponse>(cancellationToken);
        var text = result?.Candidates?[0].Content?.Parts?[0].Text;
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException($"Empty response from Gemini model {model}.");

        return GeminiResponseParser.Parse<T>(text);
    }
}
