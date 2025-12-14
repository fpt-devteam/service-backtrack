using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;


namespace Backtrack.Core.Infrastructure.DependencyInjections;

public static class SemanticKernelInstaller
{

    public static IServiceCollection AddSemanticKernelInfra(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient("ai", c =>
        {
            c.Timeout = TimeSpan.FromMinutes(5);
        });

        services.Configure<SemanticKernelSettings>(config.GetSection("SemanticKernel"));
        services.AddSingleton(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("ai");
            var opts = sp.GetRequiredService<IOptions<SemanticKernelSettings>>().Value;
            return CreateKernelByProvider(opts, http);
        });

        return services;
    }

    private static Kernel CreateKernelByProvider(SemanticKernelSettings settings, HttpClient httpClient)
    {
        return settings.Provider switch
        {
            AIProvider.OpenAI => CreateOpenAIKernel(settings, httpClient),
            AIProvider.GoogleAI => CreateGoogleAIKernel(settings, httpClient),
            AIProvider.Ollama => CreateOllamaKernel(settings, httpClient),
            _ => throw new NotSupportedException($"AI Provider '{settings.Provider}' is not supported.")
        };
    }

    private static Kernel CreateOpenAIKernel(SemanticKernelSettings settings, HttpClient httpClient)
    {
        return Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: settings.OpenAIModel,
                apiKey: settings.OpenAIApiKey,
                httpClient: httpClient
            )
            .Build();
    }

    private static Kernel CreateGoogleAIKernel(SemanticKernelSettings settings, HttpClient httpClient)
    {
        return Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(
                modelId: settings.GoogleAIModel,
                apiKey: settings.GoogleAIApiKey,
                httpClient: httpClient
            )
            .Build();
    }

    private static Kernel CreateOllamaKernel(SemanticKernelSettings settings, HttpClient httpClient)
    {
        return Kernel.CreateBuilder()
            .AddOllamaChatCompletion(
                modelId: settings.OllamaAIModel,
                endpoint: new Uri(settings.OllamaEndpoint)
            )
            .Build();
    }
}