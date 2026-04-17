namespace Backtrack.Core.Application.Interfaces.AI;

public sealed record LlmRequest
{
    public required string SystemPrompt { get; init; }
    public required string UserPrompt { get; init; }
    public string? ImageBase64 { get; init; }
    public string? ImageMimeType { get; init; }
    public float Temperature { get; init; } = 0.2f;
    public int MaxOutputTokens { get; init; } = 1024;
    /// <summary>Overrides the default generation model from GeminiSettings.</summary>
    public string? Model { get; init; }
}

public interface ILlmService
{
    /// <summary>
    /// Calls the LLM with optional image, structured system+user prompts,
    /// and deserializes the response JSON into T.
    /// </summary>
    Task<T> CompleteAsync<T>(LlmRequest request, CancellationToken cancellationToken = default) where T : class;
}
