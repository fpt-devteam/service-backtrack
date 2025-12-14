namespace Backtrack.Core.Infrastructure.DependencyInjections;

public sealed class AzureAIFoundrySettings
{
    public string DocumentIntelligenceEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 120;
}
