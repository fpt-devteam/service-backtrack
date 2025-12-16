using System.Text.Json.Serialization;

namespace Backtrack.Core.WebApi.Extensions
{
    public static class JsonNamingDI
    {
        public static void AddJsonNamingConfiguration(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        }
    }
}
