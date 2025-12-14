namespace Backtrack.ApiGateway.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;

        // Check if correlation ID already exists in request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingCorrelationId) &&
            !string.IsNullOrWhiteSpace(existingCorrelationId))
        {
            correlationId = existingCorrelationId!;
            _logger.LogDebug("Using existing Correlation ID: {CorrelationId}", correlationId);
        }
        else
        {
            // Generate new correlation ID
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId;
            _logger.LogDebug("Generated new Correlation ID: {CorrelationId}", correlationId);
        }

        // Add correlation ID to response headers for client tracking
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
            }
            return Task.CompletedTask;
        });

        // Add correlation ID to logger scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdHeader] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
