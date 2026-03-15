namespace Backtrack.Core.Infrastructure.Configurations
{
    /// <summary>
    /// Configuration settings for Google Gemini API.
    /// </summary>
    public class GeminiSettings
    {
        public string ApiKey { get; init; } = default!;

        public string BaseUrl { get; init; } = "https://generativelanguage.googleapis.com/v1beta/models";

        public string ModelName { get; init; } = "gemini-embedding-2-preview";

        public int EmbeddingDimension { get; init; } = 1536;

        public int TimeoutSeconds { get; init; } = 30;
    }
}
