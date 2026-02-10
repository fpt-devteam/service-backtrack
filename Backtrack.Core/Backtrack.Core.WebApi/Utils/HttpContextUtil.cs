namespace Backtrack.Core.WebApi.Utils
{
    public static class HttpContextUtil
    {
        public static string GetHeaderValue(HttpContext context, string headerName)
            => context.Request.Headers.TryGetValue(headerName, out var values)
                ? values.ToString()
                : string.Empty;
    }
}
