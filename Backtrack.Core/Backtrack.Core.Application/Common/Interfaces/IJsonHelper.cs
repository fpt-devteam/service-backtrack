using System.Text.Json;

namespace Backtrack.Core.Application.Common.Interfaces;

public interface IJsonHelper
{
    /// <summary>
    /// Serialize an object to JSON.
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// Deserialize JSON into an object of type T.
    /// </summary>
    T? Deserialize<T>(string json);

    /// <summary>
    /// Deserialize JSON into an object with a runtime type.
    /// </summary>
    object? Deserialize(string json, Type type);

    /// <summary>
    /// Generate JSON Schema from a generic type (T).
    /// </summary>
    string GenerateJsonSchema<T>();

    /// <summary>
    /// Generate JSON Schema from a runtime type.
    /// </summary>
    string GenerateJsonSchema(Type type);

    /// <summary>
    /// Deserialize JsonElement to an object of type T.
    /// </summary>
    T? DeserializeFromElement<T>(JsonElement element);

    /// <summary>
    /// Serialize an object to JsonElement.
    /// </summary>
    JsonElement SerializeToElement<T>(T obj);

    /// <summary>
    /// Serialize an object to JsonDocument.
    /// </summary>
    JsonDocument SerializeToDocument<T>(T obj);
}