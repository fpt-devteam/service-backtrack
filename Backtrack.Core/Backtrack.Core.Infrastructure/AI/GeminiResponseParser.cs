using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

/// <summary>
/// Utility class to parse and clean JSON responses from Google Gemini API.
/// </summary>
internal static class GeminiResponseParser
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    static GeminiResponseParser()
    {
        DefaultOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Cleans markdown formatting from the response and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="responseText">The raw response text from Gemini.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parsing fails.</exception>
    public static T Parse<T>(string responseText)
    {
        var cleaned = CleanResponse(responseText);

        try
        {
            return JsonSerializer.Deserialize<T>(cleaned, DefaultOptions)
                ?? throw new InvalidOperationException($"Failed to parse Gemini response: {cleaned}");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse Gemini response as JSON: {ex.Message}. Response: {cleaned}");
        }
    }

    /// <summary>
    /// Removes markdown code blocks (e.g., ```json ... ```) from the response text.
    /// </summary>
    /// <param name="responseText">The raw response text from Gemini.</param>
    /// <returns>The cleaned JSON string.</returns>
    public static string CleanResponse(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return string.Empty;

        var cleaned = responseText.Trim();

        if (cleaned.StartsWith("```json"))
            cleaned = cleaned[7..];
        else if (cleaned.StartsWith("```"))
            cleaned = cleaned[3..];

        if (cleaned.EndsWith("```"))
            cleaned = cleaned[..^3];

        return cleaned.Trim();
    }
}
