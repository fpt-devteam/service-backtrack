using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.Infrastructure.AI;
using Backtrack.Core.WebApi.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backtrack.Core.WebApi.Extensions
{
    public static class GeminiDI
    {
        public static void AddGemini(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GeminiSettings>(configuration.GetSection("Gemini"));

            services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>((serviceProvider, client) =>
            {
                var geminiSettings = configuration.GetSection("Gemini").Get<GeminiSettings>();
                if (geminiSettings != null)
                {
                    client.Timeout = TimeSpan.FromSeconds(geminiSettings.TimeoutSeconds);
                }
            });
        }
    }
}
