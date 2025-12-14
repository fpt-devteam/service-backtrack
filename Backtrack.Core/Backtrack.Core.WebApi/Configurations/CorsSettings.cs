namespace Backtrack.Core.WebApi.Configurations
{
    public class CorsSettings
    {
        public string[] AllowedOrigins { get; init; } = default!;
        public string[] AllowedMethods { get; init; } = default!;
        public string[] AllowedHeaders { get; init; } = default!;
        public string[] ExposedHeaders { get; init; } = default!;
        public bool AllowCredentials { get; init; }
        public int PreflightMaxAge { get; init; }
    }
}