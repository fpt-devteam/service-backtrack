namespace Backtrack.Core.Infrastructure.Helpers
{
    public class PromptSanitizeHelper
    {
        public static string SanitizeInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            return input
                .Replace("{{", "{ {")
                .Replace("}}", "} }")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Trim();
        }
    }
}