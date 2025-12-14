using System.Net;
using Backtrack.ApiGateway.Errors;
using Backtrack.Core.Application.Exceptions;
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
            "/public"
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
            context.Response.StatusCode = AuthErrors.MissingAuthHeaders.HttpStatusCode.GetHashCode();
            await context.Response.WriteAsJsonAsync(AuthErrors.MissingAuthHeaders);
            return;
        }

        var token = ExtractBearerToken(authHeader!);
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = AuthErrors.InvalidAuthHeaderFormat.HttpStatusCode.GetHashCode();
            await context.Response.WriteAsJsonAsync(AuthErrors.InvalidAuthHeaderFormat);
            return;
        }

        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            var authId = decodedToken.Uid;
            if (string.IsNullOrWhiteSpace(authId))
            {
                context.Response.StatusCode = AuthErrors.InvalidAuthToken.HttpStatusCode.GetHashCode();
                await context.Response.WriteAsJsonAsync(AuthErrors.InvalidAuthToken);
                return;
            }

            context.Request.Headers[AuthIdHeaderName] = authId;
            context.Request.Headers[AuthProviderHeaderName] = "firebase";

            await _next(context);
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError(ex, "Firebase token validation failed for path: {Path}", path);
            context.Response.StatusCode = AuthErrors.InvalidAuthToken.HttpStatusCode.GetHashCode();
            await context.Response.WriteAsJsonAsync(AuthErrors.InvalidAuthToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for path: {Path}", path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new Error(
                Code: "AuthenticationError",
                Message: "An unexpected error occurred during authentication.",
                HttpStatusCode.InternalServerError));
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
}