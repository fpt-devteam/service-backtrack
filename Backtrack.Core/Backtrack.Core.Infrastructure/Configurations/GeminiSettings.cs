namespace Backtrack.Core.Infrastructure.Configurations;

public class GeminiSettings
{
    public string ApiKey { get; init; } = default!;
    public string BaseUrl { get; init; } = "https://generativelanguage.googleapis.com/v1beta/models";
    public string ModelName { get; init; } = "gemini-embedding-001";
    public string GenerationModelName { get; init; } = "gemini-2.0-flash";
    public int EmbeddingDimension { get; init; } = 1536;
    public int TimeoutSeconds { get; init; } = 30;
}
