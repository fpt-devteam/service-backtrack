using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Contract.Common;
using Backtrack.Core.WebApi.Constants;
using FluentValidation;
using System.Diagnostics;
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

        string correlationId = context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var values)
            ? values.ToString()
            : context.TraceIdentifier;

        var camelOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string errorMessage = exception.Message;
        if (exception.InnerException != null)
        {
            errorMessage += $" Inner exception: {exception.InnerException.Message}";
        }

        ApiResponse<object> response;
        if (exception is FluentValidation.ValidationException vex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

            logger.LogWarning(vex, "Validation exception occurred. CorrelationId={CorrelationId} ErrorMessage={ErrorMessage} Errors={Errors} StackTrace={StackTrace}",
                correlationId, errorMessage, errors, vex.StackTrace);

            response = ApiResponse<object>.ErrorResponse(
                new ApiError
                {
                    Code = "ValidationError",
                    Message = "One or more validation errors occurred.",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Details = errors
                },
                correlationId
            );

            await context.Response.WriteAsJsonAsync(response, camelOption);

            return;
        }

        if (exception is Application.Common.Exceptions.ValidationException validationException)
        {
            logger.LogWarning(validationException, "Validation exception occurred. CorrelationId={CorrelationId} ErrorMessage={ErrorMessage} StackTrace={StackTrace}",
                correlationId, errorMessage, validationException.StackTrace);

            context.Response.StatusCode = (int)validationException.Error.HttpStatusCode;

            response = ApiResponse<object>.ErrorResponse(
                new ApiError
                {
                    Code = validationException.Error.Code,
                    Message = validationException.Error.Message,
                    StatusCode = (int)validationException.Error.HttpStatusCode
                },
                correlationId
            );

            await context.Response.WriteAsJsonAsync(response, camelOption);
            return;
        }

        if (exception is DomainException domainException)
        {
            logger.LogWarning(domainException, "Domain exception occurred. CorrelationId={CorrelationId} ErrorCode={ErrorCode} ErrorMessage={ErrorMessage} StackTrace={StackTrace}",
                correlationId, domainException.Error.Code, domainException.Error.Message, domainException.StackTrace);

            context.Response.StatusCode = (int)domainException.Error.HttpStatusCode;

            response = ApiResponse<object>.ErrorResponse(
                new ApiError
                {
                    Code = domainException.Error.Code,
                    Message = domainException.Error.Message,
                    StatusCode = (int)domainException.Error.HttpStatusCode
                },
                correlationId
            );

            await context.Response.WriteAsJsonAsync(response, camelOption);
            return;
        }

        logger.LogError(exception, "Unexpected exception occurred: {Message} CorrelationId={CorrelationId} StackTrace={StackTrace}",
            errorMessage, correlationId, exception.StackTrace);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        response = ApiResponse<object>.ErrorResponse(
            new ApiError
            {
                Code = "InternalServerError",
                Message = "An internal server error occurred.",
                StatusCode = StatusCodes.Status500InternalServerError
            },
            correlationId
        );

        await context.Response.WriteAsJsonAsync(response, camelOption);
    }
}