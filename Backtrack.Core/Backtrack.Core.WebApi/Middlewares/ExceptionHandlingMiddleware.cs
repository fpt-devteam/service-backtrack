using Backtrack.Core.Application.Common.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace Backtrack.Core.WebApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning(ex, "Response already started, rethrowing.");
                throw;
            }

            context.Response.Clear();
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        string? correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var values)
            ? values.ToString()
            : null;
        string? traceId = context.TraceIdentifier;

        var camelOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string errorMessage = exception.Message;
        if (exception.InnerException != null)
        {
            errorMessage += $" Inner exception: {exception.InnerException.Message}";
        }

        if (exception is FluentValidation.ValidationException vex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

            logger.LogInformation("Validation exception occurred. TraceId={TraceId} CorrelationId={CorrelationId} ErrorMessage={ErrorMessage} Errors={Errors}", traceId, correlationId, errorMessage, errors);

            await context.Response.WriteAsJsonAsync(new
            {
                code = "ValidationError",
                message = "One or more validation errors occurred.",
                errors,
                traceId,
                correlationId
            }, camelOption);

            return;
        }

        if (exception is DomainException domainException)
        {
            context.Response.StatusCode = (int)domainException.Error.HttpStatusCode;
            logger.LogInformation("Domain exception occurred. TraceId={TraceId} CorrelationId={CorrelationId} ErrorCode={ErrorCode} ErrorMessage={ErrorMessage}",
                traceId, correlationId, domainException.Error.Code, domainException.Error.Message);

            await context.Response.WriteAsJsonAsync(new
            {
                code = domainException.Error.Code,
                message = domainException.Error.Message,
                traceId,
                correlationId
            }, camelOption);
            return;
        }

        logger.LogError(exception, "Exception occurred: {Message} TraceId={TraceId} CorrelationId={CorrelationId}", errorMessage, traceId, correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            code = "InternalServerError",
            message = "An internal server error occurred.",
            traceId,
            correlationId
        }, camelOption);
    }
}