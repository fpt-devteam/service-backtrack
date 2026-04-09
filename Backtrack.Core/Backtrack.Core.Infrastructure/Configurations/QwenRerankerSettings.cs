namespace Backtrack.Core.Infrastructure.Configurations;

public sealed class QwenRerankerSettings
{
    public string BaseUrl { get; init; } = "http://localhost:8888";
    public string ModelName { get; init; } = "Qwen/Qwen3-Reranker-0.6B";
    public int TimeoutSeconds { get; init; } = 30;
}
