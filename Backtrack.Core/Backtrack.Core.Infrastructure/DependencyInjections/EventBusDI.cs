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
            var dbConnectionString = configuration.GetConnectionString("PostgresDB")
                ?? throw new InvalidOperationException("ConnectionStrings:PostgresDB is required.");
            options.UsePostgreSql(dbConnectionString);

            // Use RabbitMQ as message broker with connection string
            // Format: amqp://username:password@hostname:port/virtualhost
            var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ")
                ?? throw new InvalidOperationException("ConnectionStrings:RabbitMQ is required.");

            var exchangeName = configuration["RabbitMQ:ExchangeName"]
                ?? throw new InvalidOperationException("RabbitMQ:ExchangeName is required.");

            options.UseRabbitMQ(rabbitMqOptions =>
            {
                rabbitMqOptions.ConnectionFactoryOptions = factory =>
                {
                    factory.Uri = new Uri(rabbitMqConnectionString);
                };
                rabbitMqOptions.ExchangeName = exchangeName;
            });

            // CAP configuration
            options.DefaultGroupName = configuration["CAP:DefaultGroupName"]
                ?? throw new InvalidOperationException("CAP:DefaultGroupName is required.");

            options.FailedRetryCount = configuration.GetValue<int>("CAP:FailedRetryCount");

            options.Version = configuration["CAP:Version"]
                ?? throw new InvalidOperationException("CAP:Version is required.");

            // Configure JSON serialization
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }
}
