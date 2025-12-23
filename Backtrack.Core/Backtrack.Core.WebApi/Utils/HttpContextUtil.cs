using Backtrack.Core.WebApi.Constants;

namespace Backtrack.Core.WebApi.Utils
{
    public static class HttpContextUtil
    {
        public static string GetCorrelationId(HttpContext context)
            => context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var values)
                ? values.ToString()
                : string.Empty;
    }
}
