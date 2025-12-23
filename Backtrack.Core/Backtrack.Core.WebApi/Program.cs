using Backtrack.Core.Infrastructure.Data;
using Backtrack.Core.Infrastructure.DependencyInjections;
using Backtrack.Core.WebApi.DependencyInjections;
using Backtrack.Core.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
            var envFilePath = Path.Combine(repoRoot, "env", "backtrack-core-api.docker.env");
            if (File.Exists(envFilePath))
            {
                DotNetEnv.Env.Load(envFilePath);
                Console.WriteLine($"Loaded env from: {envFilePath}");
            }
            else
            {
                throw new FileNotFoundException($"Env file not found at {envFilePath}");
            }
        }

        builder.Configuration
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
            .AddEnvironmentVariables();

        builder.Services.AddDatabase(builder.Configuration);
        builder.Services.AddGemini(builder.Configuration);
        builder.Services.AddConfiguredCors(builder.Configuration);
        builder.Services.AddServiceConfigurations(builder.Configuration);
        builder.Services.AddHangfire(builder.Configuration);
        builder.Services.AddEventBus(builder.Configuration);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddJsonOptions();
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
