using Backtrack.Core.WebApi.Middlewares;

namespace Backtrack.Core.WebApi.DependencyInjections
{
    public static class ExceptionHandlingMiddlewareDI
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
            => builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}