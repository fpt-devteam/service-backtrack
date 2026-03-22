using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SearchMode
{
    Keyword,
    Semantic
}
