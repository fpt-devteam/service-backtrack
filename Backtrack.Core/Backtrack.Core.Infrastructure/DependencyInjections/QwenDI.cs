using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Infrastructure.AI;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backtrack.Core.Infrastructure.DependencyInjections;

public static class QwenDI
{
    public static void AddQwenReranker(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QwenRerankerSettings>(configuration.GetSection("QwenRerankerSettings"));

        services.AddHttpClient<ICrossEncoderService, QwenCrossEncoderService>((_, client) =>
        {
            var settings = configuration.GetSection("QwenRerankerSettings").Get<QwenRerankerSettings>()
                           ?? new QwenRerankerSettings();

            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
            client.Timeout     = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });
    }
}
