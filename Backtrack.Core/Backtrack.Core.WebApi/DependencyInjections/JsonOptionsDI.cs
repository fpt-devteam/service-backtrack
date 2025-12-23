using Backtrack.Core.WebApi.DependencyInjections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.WebApi.DependencyInjections
{
    public static class JsonOptionsDI
    {
        public static IServiceCollection AddJsonOptions(this IServiceCollection services)
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            return services;
        }
    }
}
