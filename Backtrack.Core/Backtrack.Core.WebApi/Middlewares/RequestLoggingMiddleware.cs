using Backtrack.Core.WebApi.Constants;
using System.Diagnostics;

public sealed class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
        => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();

        var correlationId =
            context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var cid) && !string.IsNullOrWhiteSpace(cid)
                ? cid.ToString()
                : string.Empty;

        context.Response.Headers[HeaderNames.CorrelationId] = correlationId;

        _logger.LogInformation(
            "REQ_START {CorrelationId} {Method} {Path}{QueryString}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString
        );

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();

            // Log end
            _logger.LogInformation(
                "REQ_END   {CorrelationId} {StatusCode} {ElapsedMs}ms",
                correlationId,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds
            );
        }
    }
}
