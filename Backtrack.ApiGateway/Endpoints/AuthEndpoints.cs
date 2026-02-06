using Backtrack.ApiGateway.Common;
using FirebaseAdmin.Auth;

namespace Backtrack.ApiGateway.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/check-email", CheckEmailStatus)
            .WithName("CheckEmailStatus");
    }

    private static async Task<IResult> CheckEmailStatus(
        CheckEmailRequest request,
        HttpContext httpContext,
        ILogger<Program> logger)
    {
        var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? httpContext.TraceIdentifier;

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                new ApiError
                {
                    Code = "InvalidEmail",
                    Message = "Email is required."
                },
                correlationId);
            return Results.BadRequest(errorResponse);
        }

        try
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(request.Email);

            var result = new CheckEmailResult
            {
                Status = userRecord.EmailVerified
                    ? EmailStatus.Verified
                    : EmailStatus.NotVerified,
                Email = userRecord.Email
            };

            var response = ApiResponse<CheckEmailResult>.SuccessResponse(result, correlationId);
            return Results.Ok(response);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            var result = new CheckEmailResult
            {
                Status = EmailStatus.NotFound,
                Email = request.Email
            };

            var response = ApiResponse<CheckEmailResult>.SuccessResponse(result, correlationId);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking email status for: {Email}", request.Email);

            var errorResponse = ApiResponse<object>.ErrorResponse(
                new ApiError
                {
                    Code = "InternalError",
                    Message = "An error occurred while checking email status."
                },
                correlationId);
            return Results.StatusCode(500);
        }
    }
}

public record CheckEmailRequest
{
    public required string Email { get; init; }
}

public record CheckEmailResult
{
    public required EmailStatus Status { get; init; }
    public required string Email { get; init; }
}

public enum EmailStatus
{
    /// <summary>
    /// Account exists and email is verified
    /// </summary>
    Verified,

    /// <summary>
    /// Account exists but email is not verified
    /// </summary>
    NotVerified,

    /// <summary>
    /// Account does not exist
    /// </summary>
    NotFound
}
