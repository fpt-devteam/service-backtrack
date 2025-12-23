using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backtrack.Core.Infrastructure.DependencyInjections;

public static class EventBusDI
{
    public static void AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCap(options =>
        {
            // Use PostgreSQL for outbox pattern (transactional messaging)
            options.UsePostgreSql(postgresOptions =>
            {
                postgresOptions.ConnectionString = configuration.GetConnectionString("DefaultConnection");
            });

            // Use RabbitMQ as message broker
            options.UseRabbitMQ(rabbitmqOptions =>
            {
                rabbitmqOptions.HostName = configuration["RabbitMQ:HostName"] ?? "localhost";
                rabbitmqOptions.Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
                rabbitmqOptions.UserName = configuration["RabbitMQ:UserName"] ?? "guest";
                rabbitmqOptions.Password = configuration["RabbitMQ:Password"] ?? "guest";
                rabbitmqOptions.VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
                rabbitmqOptions.ExchangeName = configuration["RabbitMQ:ExchangeName"] ?? "backtrack.events";
            });

            // CAP configuration
            options.DefaultGroupName = configuration["CAP:DefaultGroupName"] ?? "backtrack.core";
            options.FailedRetryCount = int.Parse(configuration["CAP:FailedRetryCount"] ?? "3");
            options.Version = configuration["CAP:Version"] ?? "v1";

            // Optional: Configure JSON serialization
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }
}
