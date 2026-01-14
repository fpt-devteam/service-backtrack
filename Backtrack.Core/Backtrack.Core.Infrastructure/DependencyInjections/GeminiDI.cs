using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.Infrastructure.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backtrack.Core.Infrastructure.DependencyInjections
{
    public static class GeminiDI
    {
        public static void AddGemini(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));

            services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>((serviceProvider, client) =>
            {
                var geminiSettings = configuration.GetSection("GeminiSettings").Get<GeminiSettings>();
                if (geminiSettings != null)
                {
                    client.Timeout = TimeSpan.FromSeconds(geminiSettings.TimeoutSeconds);
                }
            });

            services.AddHttpClient<IImageAnalysisService, GeminiImageAnalysisService>((serviceProvider, client) =>
            {
                var geminiSettings = configuration.GetSection("GeminiSettings").Get<GeminiSettings>();
                if (geminiSettings != null)
                {
                    // Image analysis may take longer due to image processing
                    client.Timeout = TimeSpan.FromSeconds(geminiSettings.TimeoutSeconds * 2);
                }
            });
        }
    }
}
