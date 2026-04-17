using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Infrastructure.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Backtrack.Core.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backtrack.Core.Infrastructure.DependencyInjections;

public static class GeminiDI
{
    public static void AddGemini(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));

        // Core LLM service — single HttpClient for all generateContent calls
        services.AddHttpClient<ILlmService, GeminiLlmService>((_, client) =>
        {
            var s = configuration.GetSection("GeminiSettings").Get<GeminiSettings>();
            if (s != null) client.Timeout = TimeSpan.FromSeconds(s.TimeoutSeconds * 3);
        });

        // Embedding service — separate HttpClient (different endpoint/model)
        services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>((_, client) =>
        {
            var s = configuration.GetSection("GeminiSettings").Get<GeminiSettings>();
            if (s != null) client.Timeout = TimeSpan.FromSeconds(s.TimeoutSeconds);
        });

        // Services that consume ILlmService (resolved via DI, no own HttpClient)
        services.AddScoped<IImageAnalysisService, GeminiImageAnalysisService>();
        services.AddScoped<IPostMatchAssessor, GeminiPostMatchAssessor>();
        services.AddScoped<IOcrService, GeminiOcrService>();

        // Cross-encoder: also consumes ILlmService
        services.AddScoped<ICrossEncoderService, GeminiCrossEncoderService>();

        services.AddHttpClient<IImageFetcher, HttpImageFetcher>();
    }
}
