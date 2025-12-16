using Json.Schema;
using Json.Schema.Generation;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backtrack.Core.Application.Common.Interfaces.Helpers;

namespace Backtrack.Core.Infrastructure.Helpers
{
    public class JsonHelper : IJsonHelper
    {
        // Default System.Text.Json settings:
        // - CamelCase property names
        // - Enums serialized as string (camelCase)
        // - Null values ignored (to keep JSON compact)
        private static readonly JsonSerializerOptions DefaultSettings = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        /// <summary>
        /// Serialize an object to JSON using System.Text.Json.
        /// </summary>
        public string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, DefaultSettings);
        }

        /// <summary>
        /// Deserialize JSON into an object of type T using System.Text.Json.
        /// </summary>
        public T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, DefaultSettings);
        }

        /// <summary>
        /// Deserialize JSON into an object with a runtime type using System.Text.Json.
        /// </summary>
        public object? Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, DefaultSettings);
        }

        /// <summary>
        /// Deserialize JsonElement to an object of type T.
        /// </summary>
        public T? DeserializeFromElement<T>(JsonElement element)
        {
            return JsonSerializer.Deserialize<T>(element.GetRawText(), DefaultSettings);
        }

        /// <summary>
        /// Serialize an object to JsonElement.
        /// </summary>
        public JsonElement SerializeToElement<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj, DefaultSettings);
            var document = JsonDocument.Parse(json);
            return document.RootElement;
        }

        /// <summary>
        /// Serialize an object to JsonDocument.
        /// </summary>
        public JsonDocument SerializeToDocument<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj, DefaultSettings);
            return JsonDocument.Parse(json);
        }

        // =========================
        // JSON SCHEMA (JsonSchema.Net)
        // =========================

        /// <summary>
        /// Generate JSON Schema from a generic type (T) using JsonSchema.Net.
        /// </summary>
        public string GenerateJsonSchema<T>()
        {
            var schema = new JsonSchemaBuilder().FromType<T>().Build();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(schema, options);
        }

        /// <summary>
        /// Generate JSON Schema from a runtime type using JsonSchema.Net.
        /// </summary>
        public string GenerateJsonSchema(Type type)
        {
            var schema = new JsonSchemaBuilder().FromType(type).Build();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(schema, options);
        }
    }
}