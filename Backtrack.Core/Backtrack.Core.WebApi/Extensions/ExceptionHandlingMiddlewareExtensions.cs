using Backtrack.Core.WebApi.Middleware;

namespace Backtrack.Core.WebApi.Extensions
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
            => builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}