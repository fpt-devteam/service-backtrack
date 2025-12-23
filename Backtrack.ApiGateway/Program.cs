using Backtrack.ApiGateway.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "backtrack-api-gateway.local.env");
    if (File.Exists(envFilePath))
        DotNetEnv.Env.Load(envFilePath);
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("FirebaseInit");
InitializeFirebase(builder.Configuration, builder.Environment, builder.Services, logger);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure middleware pipeline (order matters!)
// 1. Correlation ID - track all requests
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Health check endpoint (before auth)
app.MapHealthChecks("/health");

// 3. Firebase Authentication - validates tokens and injects user headers
app.UseMiddleware<FirebaseAuthMiddleware>();

app.MapReverseProxy();

await app.RunAsync();

static void InitializeFirebase(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services, ILogger logger)
{
    try
    {
        var firebaseConfigPath = configuration["Firebase:ServiceAccountPath"];
        var projectId = configuration["Firebase:ProjectId"];

        if (!string.IsNullOrWhiteSpace(firebaseConfigPath) && File.Exists(firebaseConfigPath))
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebaseConfigPath),
                ProjectId = projectId
            });
            logger.LogInformation("Firebase initialized from file {Path}", firebaseConfigPath);

        }
        else
        {
            throw new FileNotFoundException("Firebase service account file not found.", firebaseConfigPath);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize Firebase: {Message}", ex.Message);
        throw;
    }
}