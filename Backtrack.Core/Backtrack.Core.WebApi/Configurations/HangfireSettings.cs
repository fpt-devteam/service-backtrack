namespace Backtrack.Core.WebApi.Configurations
{
    public class HangfireSettings
    {
        public bool EnableDashboard { get; init; } = true;
        public string DashboardPath { get; init; } = "/hangfire";
        public int WorkerCount { get; init; } = 5;
        public string[] DashboardAllowedIPs { get; init; } = Array.Empty<string>();
    }
}
