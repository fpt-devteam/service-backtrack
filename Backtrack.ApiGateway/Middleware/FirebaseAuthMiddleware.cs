using System.Net;
using Backtrack.ApiGateway.Common;
using Backtrack.ApiGateway.Errors;
using Backtrack.ApiGateway.Exceptions;
using FirebaseAdmin.Auth;

namespace Backtrack.ApiGateway.Middleware;

public class FirebaseAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FirebaseAuthMiddleware> _logger;
    private readonly HashSet<string> _publicPaths;
    private const string AuthHeaderName = "Authorization";
    private const string AuthIdHeaderName = "X-Auth-Id";
    private const string AuthProviderHeaderName = "X-Auth-Provider";
    private const string AuthEmailHeaderName = "X-Auth-Email";
    private const string AuthNameHeaderName = "X-Auth-Name";
    private const string AuthAvatarUrlHeaderName = "X-Auth-Avatar-Url";
    private const string CorrelationIdHeaderName = "X-Correlation-Id";



    public FirebaseAuthMiddleware(
        RequestDelegate next,
        ILogger<FirebaseAuthMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        _publicPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/health",
            "/api/qr/qr-code/public-code",
            "/api/qr/health",
            "/api/chat/hub",
            "/api/qr/order/payment-webhook"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(AuthHeaderName, out var authHeader) ||
            string.IsNullOrWhiteSpace(authHeader))
        {
            await WriteErrorResponse(context, AuthErrors.MissingAuthHeaders, StatusCodes.Status401Unauthorized);
            return;
        }

        var token = ExtractBearerToken(authHeader!);
        if (string.IsNullOrWhiteSpace(token))
        {
            await WriteErrorResponse(context, AuthErrors.InvalidAuthHeaderFormat, StatusCodes.Status401Unauthorized);
            return;
        }

        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            var authId = decodedToken.Uid;
            if (string.IsNullOrWhiteSpace(authId))
            {
                await WriteErrorResponse(context, AuthErrors.InvalidAuthToken, StatusCodes.Status401Unauthorized);
                return;
            }

            var email = decodedToken.Claims.TryGetValue("email", out var emailClaim)
                ? emailClaim.ToString()
                : string.Empty;

            // if (string.IsNullOrWhiteSpace(email))
            // {
            //     _logger.LogWarning("Email missing in Firebase token for user: {UserId}", authId);
            //     await WriteErrorResponse(context, AuthErrors.MissingEmailInToken, StatusCodes.Status401Unauthorized);
            //     return;
            // }

            var displayName = decodedToken.Claims.TryGetValue("name", out var nameClaim)
                ? nameClaim.ToString()
                : string.Empty;

            var avatarUrl = decodedToken.Claims.TryGetValue("picture", out var pictureClaim)
                ? pictureClaim.ToString()
                : string.Empty;

            context.Request.Headers[AuthIdHeaderName] = authId;
            context.Request.Headers[AuthProviderHeaderName] = "firebase";
            context.Request.Headers[AuthEmailHeaderName] = email;

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                var encodedName = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(displayName));
                context.Request.Headers[AuthNameHeaderName] = encodedName;
            }

            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                var encodedAvatarUrl = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(avatarUrl));
                context.Request.Headers[AuthAvatarUrlHeaderName] = encodedAvatarUrl;
            }

            await _next(context);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError(ex, "Firebase token validation failed for path: {Path}", path);
            await WriteErrorResponse(context, AuthErrors.InvalidAuthToken, StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for path: {Path}", path);
            var internalError = new Error(
                Code: "AuthenticationError",
                Message: "An unexpected error occurred during authentication.");
            await WriteErrorResponse(context, internalError, StatusCodes.Status500InternalServerError);
        }
    }

    private bool IsPublicPath(string path)
    {
        return _publicPaths.Any(publicPath =>
            path.Equals(publicPath, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith($"{publicPath}/", StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractBearerToken(string authHeader)
    {
        const string bearerPrefix = "Bearer ";
        if (authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring(bearerPrefix.Length).Trim();
        }
        return null;
    }

    private static async Task WriteErrorResponse(HttpContext context, Error error, int statusCode)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdValue)
            ? correlationIdValue.ToString()
            : context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var apiError = new ApiError
        {
            Code = error.Code,
            Message = error.Message,
            Details = null
        };

        var response = ApiResponse<object>.ErrorResponse(apiError, correlationId);
        await context.Response.WriteAsJsonAsync(response);
    }
}