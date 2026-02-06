using System.Text.Json.Serialization;
using Backtrack.ApiGateway.Endpoints;
using Backtrack.ApiGateway.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
    var envFilePath = Path.Combine(repoRoot, "env", "backtrack-api-gateway.docker.env");
    if (File.Exists(envFilePath))
    {
        DotNetEnv.Env.Load(envFilePath);
        Console.WriteLine($"Loaded env from: {envFilePath}");
    }
    else
    {
        Console.WriteLine($"Env file not found at {envFilePath}");
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JSON serialization to return enums as strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("FirebaseInit");
InitializeFirebase(builder.Configuration, builder.Environment, builder.Services, logger);
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseWebSockets();

// Configure middleware pipeline (order matters!)
// 1. CORS - must be first to handle preflight requests
app.UseCors();

// 2. Correlation ID - track all requests
app.UseMiddleware<CorrelationIdMiddleware>();

// 3. Health check endpoint (before auth)
app.MapHealthChecks("/health");

// 4. Firebase Authentication - validates tokens and injects user headers
app.UseMiddleware<FirebaseAuthMiddleware>();

// 5. Map auth endpoints (public, before reverse proxy)
app.MapAuthEndpoints();

app.MapReverseProxy();

await app.RunAsync();

static void InitializeFirebase(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services, ILogger logger)
{
    try
    {
        var jsonBase64 = configuration["Firebase:ServiceAccountJsonBase64"];
        var projectId = configuration["Firebase:ProjectId"];

        if (!string.IsNullOrWhiteSpace(jsonBase64))
        {
            var jsonBytes = Convert.FromBase64String(jsonBase64);
            using var ms = new MemoryStream(jsonBytes);

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromStream(ms),
                ProjectId = projectId
            });

            logger.LogInformation("Firebase initialized from ServiceAccountJsonBase64 env/config.");
        }
        else
        {
            throw new InvalidOperationException(
            "Firebase credentials not found. Set Firebase:ServiceAccountJsonBase64.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize Firebase: {Message}", ex.Message);
        throw;
    }
}