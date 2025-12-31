namespace Backtrack.Core.Infrastructure.Configurations
{
    public class ConnectionStringSettings
    {
        public string PostgresDB { get; init; } = default!;
        public string RabbitMQ { get; init; } = default!;
    }
}