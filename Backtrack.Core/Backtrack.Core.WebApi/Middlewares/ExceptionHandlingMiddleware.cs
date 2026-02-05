using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Json;
using System.Diagnostics;
using System.Text.Json;
using Backtrack.Core.WebApi.Utils;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.WebApi.Common;

namespace Backtrack.Core.WebApi.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IOptions<JsonOptions> jsonOptions)
{
    private static readonly Dictionary<Type, int> StatusMap = new()
    {
        [typeof(NotFoundException)] = StatusCodes.Status404NotFound,
        [typeof(ConflictException)] = StatusCodes.Status409Conflict,
        [typeof(UnauthorizedException)] = StatusCodes.Status401Unauthorized,
        [typeof(Application.Exceptions.ValidationException)] = StatusCodes.Status400BadRequest,
        [typeof(FluentValidation.ValidationException)] = StatusCodes.Status400BadRequest,
    };

    private readonly JsonSerializerOptions _json = jsonOptions.Value.SerializerOptions;

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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var correlationId = HttpContextUtil.GetCorrelationId(context);
        var status = ResolveStatusCode(ex) ?? StatusCodes.Status500InternalServerError;

        if (status >= 500)
            logger.LogError(ex, "Unexpected exception. CorrelationId={CorrelationId}", correlationId);
        else
            logger.LogWarning(ex.Message, "Handled exception. CorrelationId={CorrelationId}", correlationId);

        var apiError = BuildApiError(ex, status);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.ErrorResponse(apiError, correlationId),
            _json
        );

    }

    private static ApiError BuildApiError(Exception ex, int status)
    {
        if (ex is FluentValidation.ValidationException fvex)
        {
            var details = fvex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

            return new ApiError
            {
                Code = "ValidationError",
                Message = "One or more validation errors occurred.",
                Details = details
            };
        }

        if (ex is DomainException dex)
        {
            return new ApiError
            {
                Code = dex.Error.Code,
                Message = dex.Error.Message,
                Details = null
            };
        }

        return new ApiError
        {
            Code = "InternalServerError",
            Message = "An internal server error occurred.",
            Details = null
        };
    }

    private static int? ResolveStatusCode(Exception ex)
    {
        if (StatusMap.TryGetValue(ex.GetType(), out var exact)) return exact;
        return null;
    }
}