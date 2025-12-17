using Backtrack.Core.Infrastructure.Data;
using Backtrack.Core.WebApi.Extensions;
using Backtrack.Core.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "backtrack-core.local.env");
            if (File.Exists(envFilePath))
            {
                DotNetEnv.Env.Load(envFilePath);
            }

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddGemini(builder.Configuration);
            builder.Services.AddConfiguredCors(builder.Configuration);
            builder.Services.AddServiceConfigurations(builder.Configuration);
            builder.Services.AddHangfire(builder.Configuration);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddJsonNamingConfiguration();
            builder.Services.AddConfiguredSwagger();
            builder.Services.AddHealthChecks();

            WebApplication app = builder.Build();

            await MigrateDatabaseAsync(app);

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.MapHealthChecks("/health");

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("AppCorsPolicy");
            app.UseHangfireDashboardIfEnabled(builder.Configuration);
            app.UseHttpsRedirection();
            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task MigrateDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Starting database migration...");

                await context.Database.MigrateAsync();

                logger.LogInformation("Database migration completed successfully.");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
                throw;
            }
        }
    }
}
