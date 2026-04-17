using System.Text.Json.Serialization;

namespace Backtrack.Core.Infrastructure.AI;

internal sealed class GeminiGenerateRequest
{
    [JsonPropertyName("systemInstruction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiGenerateContent? SystemInstruction { get; set; }

    [JsonPropertyName("contents")]
    public List<GeminiGenerateContent> Contents { get; set; } = [];

    [JsonPropertyName("generationConfig")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiGenerateConfig? GenerationConfig { get; set; }
}

internal sealed class GeminiGenerateContent
{
    [JsonPropertyName("parts")]
    public List<GeminiGeneratePart> Parts { get; set; } = [];
}

internal sealed class GeminiGeneratePart
{
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }

    [JsonPropertyName("inlineData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiInlineData? InlineData { get; set; }
}

internal sealed class GeminiInlineData
{
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

internal sealed class GeminiGenerateConfig
{
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }

    [JsonPropertyName("maxOutputTokens")]
    public int MaxOutputTokens { get; set; }
}

internal sealed class GeminiGenerateResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiGenerateCandidate>? Candidates { get; set; }
}

internal sealed class GeminiGenerateCandidate
{
    [JsonPropertyName("content")]
    public GeminiGenerateContent? Content { get; set; }
}
