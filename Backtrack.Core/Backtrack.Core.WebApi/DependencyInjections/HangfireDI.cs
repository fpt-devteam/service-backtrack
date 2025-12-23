using Backtrack.Core.Application.Common.Interfaces.BackgroundJobs;
using Backtrack.Core.Infrastructure.BackgroundJobs;
using Backtrack.Core.WebApi.Configurations;
using Backtrack.Core.WebApi.DependencyInjections;
using Hangfire;
using Hangfire.PostgreSql;

namespace Backtrack.Core.WebApi.DependencyInjections
{
    public static class HangfireDI
    {
        public static IServiceCollection AddHangfire(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<HangfireSettings>(
                configuration.GetSection("Hangfire"));

            var hangfireSettings = configuration
                .GetSection("Hangfire")
                .Get<HangfireSettings>() ?? new HangfireSettings();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Database connection string not found");

            // Configure Hangfire to use PostgreSQL storage
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(connectionString)));

            // Add Hangfire server with configured worker count
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = hangfireSettings.WorkerCount;
            });

            // Register background job service implementation
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

            return services;
        }

        public static IApplicationBuilder UseHangfireDashboardIfEnabled(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            var hangfireSettings = configuration
                .GetSection("Hangfire")
                .Get<HangfireSettings>() ?? new HangfireSettings();

            if (hangfireSettings.EnableDashboard)
            {
                app.UseHangfireDashboard(hangfireSettings.DashboardPath);
            }

            return app;
        }
    }
}
