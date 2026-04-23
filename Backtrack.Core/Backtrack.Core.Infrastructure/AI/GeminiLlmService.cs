using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace Backtrack.Core.Infrastructure.AI;

public sealed class GeminiLlmService(
    HttpClient httpClient,
    IOptions<GeminiSettings> settings,
    ILogger<GeminiLlmService> logger) : ILlmService
{
    private const int MaxParseRetries = 3;
    private readonly GeminiSettings _settings = settings.Value;

    public async Task<T> CompleteAsync<T>(LlmRequest request, CancellationToken cancellationToken = default) where T : class
    {
        var model = string.IsNullOrWhiteSpace(request.Model)
            ? _settings.GenerationModelName
            : request.Model;

        var url = $"{_settings.BaseUrl}/{model}:generateContent?key={_settings.ApiKey}";
        var geminiRequest = BuildRequest(request);

        for (var attempt = 1; attempt <= MaxParseRetries; attempt++)
        {
            var response = await httpClient.PostAsJsonAsync(url, geminiRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Gemini generateContent failed (model={Model}, status={Status}): {Error}", model, (int)response.StatusCode, error);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new ValidationException(AiErrors.Busy);

                response.EnsureSuccessStatusCode();
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiGenerateResponse>(cancellationToken);
            var candidate = result?.Candidates?.FirstOrDefault();
            var text = candidate?.Content?.Parts?.FirstOrDefault()?.Text;

            var finishReason = candidate?.FinishReason;
            if (!string.IsNullOrWhiteSpace(finishReason) && finishReason != "STOP")
                logger.LogWarning("Gemini finished with reason '{FinishReason}' on attempt {Attempt}/{Max} — response may be incomplete.", finishReason, attempt, MaxParseRetries);

            if (string.IsNullOrWhiteSpace(text))
            {
                logger.LogWarning("Gemini returned empty text on attempt {Attempt}/{Max}.", attempt, MaxParseRetries);
                if (attempt == MaxParseRetries)
                    throw new ValidationException(AiErrors.InvalidResponse);
                continue;
            }

            try
            {
                return GeminiResponseParser.Parse<T>(text);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse Gemini response on attempt {Attempt}/{Max}. Raw: {Text}", attempt, MaxParseRetries, text);
                if (attempt == MaxParseRetries)
                    throw new ValidationException(AiErrors.InvalidResponse);
            }
        }

        throw new ValidationException(AiErrors.InvalidResponse);
    }

    private static GeminiGenerateRequest BuildRequest(LlmRequest request)
    {
        var parts = new List<GeminiGeneratePart>
        {
            new() { Text = request.UserPrompt }
        };

        if (request.Images is { Count: > 0 })
        {
            foreach (var img in request.Images)
                parts.Add(new GeminiGeneratePart { InlineData = new GeminiInlineData { MimeType = img.MimeType, Data = img.Base64 } });
        }
        else if (!string.IsNullOrWhiteSpace(request.ImageBase64) && !string.IsNullOrWhiteSpace(request.ImageMimeType))
        {
            parts.Add(new GeminiGeneratePart { InlineData = new GeminiInlineData { MimeType = request.ImageMimeType, Data = request.ImageBase64 } });
        }

        return new GeminiGenerateRequest
        {
            SystemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null
                : new GeminiGenerateContent { Parts = [new GeminiGeneratePart { Text = request.SystemPrompt }] },
            Contents = [new GeminiGenerateContent { Parts = parts }],
            GenerationConfig = new GeminiGenerateConfig
            {
                Temperature     = request.Temperature,
                MaxOutputTokens = request.MaxOutputTokens,
                // Constrained JSON decoding: guarantees a complete, valid JSON object.
                // Without this, Gemini can stop mid-JSON when hitting MaxOutputTokens.
                ResponseMimeType = "application/json"
            }
        };
    }
}
