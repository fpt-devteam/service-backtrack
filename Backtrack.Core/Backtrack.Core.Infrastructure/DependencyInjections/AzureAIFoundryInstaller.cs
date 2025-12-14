using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Backtrack.Core.Infrastructure.DependencyInjections;

public static class AzureAIFoundryInstaller
{
    public static IServiceCollection AddAzureAIFoundry(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AzureAIFoundrySettings>(config.GetSection("AzureAIFoundrySettings"));

        services.AddSingleton<DocumentIntelligenceClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AzureAIFoundrySettings>>().Value;

            if (string.IsNullOrWhiteSpace(settings.DocumentIntelligenceEndpoint))
                throw new ArgumentException("Azure Document Intelligence endpoint is required");

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
                throw new ArgumentException("Azure AI Foundry API key is required");

            var credential = new AzureKeyCredential(settings.ApiKey);
            return new DocumentIntelligenceClient(new Uri(settings.DocumentIntelligenceEndpoint), credential);
        });

        return services;
    }
}
