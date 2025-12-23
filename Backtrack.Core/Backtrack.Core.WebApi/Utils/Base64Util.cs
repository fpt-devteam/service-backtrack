using System.Text;

namespace Backtrack.Core.WebApi.Utils;

public static class Base64Util
{
    /// <summary>
    /// Decodes a Base64-encoded string to UTF-8 text.
    /// </summary>
    /// <param name="base64String">The Base64-encoded string to decode.</param>
    /// <returns>The decoded UTF-8 string.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is not a valid Base64 string.</exception>
    public static string DecodeToUtf8(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return string.Empty;

        try
        {
            var decodedBytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(decodedBytes);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"Invalid Base64 string: {base64String}", nameof(base64String), ex);
        }
    }
}
