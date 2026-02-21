using System.Net;
using Backtrack.ApiGateway.Common;
using Backtrack.ApiGateway.Errors;
using Backtrack.ApiGateway.Exceptions;
using Backtrack.ApiGateway.Utils;
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
            "/auth/check-email",
            "/api/qr/qr-code/public-code",
            "/api/qr/health",
            "/api/chat/hub",
            "/api/qr/webhooks/stripe",
            "/api/core/swagger",
            "/swagger"
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

            var authId = GetAuthId(decodedToken);
            if (authId is null)
            {
                await WriteErrorResponse(context, AuthErrors.InvalidAuthToken, StatusCodes.Status401Unauthorized);
                return;
            }

            var email = GetEmail(decodedToken, authId);
            if (email is null)
            {
                await WriteErrorResponse(context, AuthErrors.MissingEmailInToken, StatusCodes.Status401Unauthorized);
                return;
            }

            if (!IsEmailVerified(decodedToken, authId))
            {
                await WriteErrorResponse(context, AuthErrors.EmailNotVerified, StatusCodes.Status401Unauthorized);
                return;
            }

            context.Request.Headers[AuthIdHeaderName] = authId;
            context.Request.Headers[AuthProviderHeaderName] = "firebase";
            context.Request.Headers[AuthEmailHeaderName] = email;
            context.Request.Headers[AuthNameHeaderName] = GetDisplayName(decodedToken);
            context.Request.Headers[AuthAvatarUrlHeaderName] = GetAvatarUrl(decodedToken);

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

    private static string? GetAuthId(FirebaseToken decodedToken)
    {
        var authId = decodedToken.Uid;
        return string.IsNullOrWhiteSpace(authId) ? null : authId;
    }

    private string? GetEmail(FirebaseToken decodedToken, string authId)
    {
        var email = decodedToken.Claims.TryGetValue("email", out var emailClaim)
            ? emailClaim?.ToString()
            : null;

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Email missing in Firebase token for user: {UserId}", authId);
            return null;
        }

        return email;
    }

    private bool IsEmailVerified(FirebaseToken decodedToken, string authId)
    {
        if (!decodedToken.Claims.TryGetValue("email_verified", out var emailVerifiedClaim))
        {
            _logger.LogWarning("Email verified claim missing in Firebase token for user: {UserId}", authId);
            return false;
        }

        if (emailVerifiedClaim is bool verified)
            return verified;

        if (bool.TryParse(emailVerifiedClaim?.ToString(), out var parsedVerified))
            return parsedVerified;

        _logger.LogWarning("Invalid email_verified claim value in Firebase token for user: {UserId}", authId);
        return false;
    }

    private static string GetDisplayName(FirebaseToken decodedToken)
    {
        if (!decodedToken.Claims.TryGetValue("name", out var nameClaim))
            return string.Empty;

        var name = nameClaim?.ToString() ?? string.Empty;
        return Base64Util.EncodeToBase64(name);
    }

    private static string GetAvatarUrl(FirebaseToken decodedToken)
    {
        if (!decodedToken.Claims.TryGetValue("picture", out var pictureClaim))
            return string.Empty;

        var avatarUrl = pictureClaim?.ToString() ?? string.Empty;
        return Base64Util.EncodeToBase64(avatarUrl);
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
