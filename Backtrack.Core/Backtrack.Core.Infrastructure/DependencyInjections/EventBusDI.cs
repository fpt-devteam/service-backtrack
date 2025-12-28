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
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required but not configured.");
                }
                postgresOptions.ConnectionString = connectionString;
            });

            // Use RabbitMQ as message broker
            options.UseRabbitMQ(rabbitmqOptions =>
            {
                var hostName = configuration["RabbitMQ:HostName"];
                if (string.IsNullOrWhiteSpace(hostName))
                {
                    throw new InvalidOperationException("RabbitMQ:HostName is required but not configured.");
                }
                rabbitmqOptions.HostName = hostName;

                var portString = configuration["RabbitMQ:Port"];
                if (string.IsNullOrWhiteSpace(portString) || !int.TryParse(portString, out var port))
                {
                    throw new InvalidOperationException("RabbitMQ:Port is required and must be a valid integer.");
                }
                rabbitmqOptions.Port = port;

                var userName = configuration["RabbitMQ:UserName"];
                if (string.IsNullOrWhiteSpace(userName))
                {
                    throw new InvalidOperationException("RabbitMQ:UserName is required but not configured.");
                }
                rabbitmqOptions.UserName = userName;

                var password = configuration["RabbitMQ:Password"];
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException("RabbitMQ:Password is required but not configured.");
                }
                rabbitmqOptions.Password = password;

                var virtualHost = configuration["RabbitMQ:VirtualHost"];
                if (string.IsNullOrWhiteSpace(virtualHost))
                {
                    throw new InvalidOperationException("RabbitMQ:VirtualHost is required but not configured.");
                }
                rabbitmqOptions.VirtualHost = virtualHost;

                var exchangeName = configuration["RabbitMQ:ExchangeName"];
                if (string.IsNullOrWhiteSpace(exchangeName))
                {
                    throw new InvalidOperationException("RabbitMQ:ExchangeName is required but not configured.");
                }
                rabbitmqOptions.ExchangeName = exchangeName;
            });

            // CAP configuration
            var defaultGroupName = configuration["CAP:DefaultGroupName"];
            if (string.IsNullOrWhiteSpace(defaultGroupName))
            {
                throw new InvalidOperationException("CAP:DefaultGroupName is required but not configured.");
            }
            options.DefaultGroupName = defaultGroupName;

            var failedRetryCountString = configuration["CAP:FailedRetryCount"];
            if (string.IsNullOrWhiteSpace(failedRetryCountString) || !int.TryParse(failedRetryCountString, out var failedRetryCount))
            {
                throw new InvalidOperationException("CAP:FailedRetryCount is required and must be a valid integer.");
            }
            options.FailedRetryCount = failedRetryCount;

            var version = configuration["CAP:Version"];
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new InvalidOperationException("CAP:Version is required but not configured.");
            }
            options.Version = version;

            // Optional: Configure JSON serialization
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }
}
