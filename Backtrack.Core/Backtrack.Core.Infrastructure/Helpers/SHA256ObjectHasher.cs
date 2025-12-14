using Backtrack.Core.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Backtrack.Core.Infrastructure.Helpers
{
    /// <summary>
    /// SHA256-based implementation of object hashing.
    /// Provides deterministic, cryptographically secure hashes for objects.
    /// </summary>
    public class SHA256ObjectHasher : IObjectHasher
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public SHA256ObjectHasher()
        {
            // Configure JSON serialization for consistent hashing
            _jsonOptions = new JsonSerializerOptions
            {
                // Ensure consistent property ordering
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false, // Compact format for consistent hashing
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = false // Exact case matching for consistency
            };
        }

        /// <summary>
        /// Generates a SHA256 hash from any object by serializing it to JSON first.
        /// </summary>
        /// <typeparam name="T">The type of object to hash</typeparam>
        /// <param name="obj">The object to hash</param>
        /// <returns>A 64-character hexadecimal SHA256 hash string</returns>
        public string Hash<T>(T obj)
        {
            if (obj == null)
                return Hash("null");

            // Serialize object to JSON for consistent representation
            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            return Hash(json);
        }

        /// <summary>
        /// Generates a SHA256 hash from a string input.
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>A 64-character hexadecimal SHA256 hash string</returns>
        public string Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return ComputeHash(string.Empty);

            return ComputeHash(input);
        }

        /// <summary>
        /// Generates a SHA256 hash from multiple objects by combining their JSON representations.
        /// Objects are concatenated with a delimiter to ensure uniqueness.
        /// </summary>
        /// <param name="objects">The objects to hash together</param>
        /// <returns>A 64-character hexadecimal SHA256 hash string</returns>
        public string HashMultiple(params object[] objects)
        {
            if (objects == null || objects.Length == 0)
                return Hash("empty");

            var combinedJson = new StringBuilder();

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null)
                {
                    combinedJson.Append("null");
                }
                else
                {
                    var json = JsonSerializer.Serialize(objects[i], _jsonOptions);
                    combinedJson.Append(json);
                }

                // Add delimiter between objects (except for the last one)
                if (i < objects.Length - 1)
                {
                    combinedJson.Append("|#|"); // Unique delimiter
                }
            }

            return Hash(combinedJson.ToString());
        }

        /// <summary>
        /// Computes the actual SHA256 hash from a string input.
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>A 64-character hexadecimal SHA256 hash string</returns>
        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(inputBytes);

            // Convert to hexadecimal string
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // Lowercase hex
            }

            return sb.ToString();
        }
    }
}