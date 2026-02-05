namespace Backtrack.Core.Application.Interfaces.Helpers
{
    /// <summary>
    /// Interface for generating hash values from objects and strings.
    /// Used for content hashing, caching, and data integrity verification.
    /// </summary>
    public interface IHasher
    {
        /// <summary>
        /// Generates a hash string from any object.
        /// </summary>
        /// <typeparam name="T">The type of object to hash</typeparam>
        /// <param name="obj">The object to hash</param>
        /// <returns>A hash string representing the object</returns>
        string Hash<T>(T obj);

        /// <summary>
        /// Generates a hash string from a plain string input.
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>A hash string representing the input</returns>
        string Hash(string input);

        /// <summary>
        /// Generates a hash string from multiple objects combined.
        /// Useful for creating composite hashes from multiple data sources.
        /// </summary>
        /// <param name="objects">The objects to hash together</param>
        /// <returns>A hash string representing all objects combined</returns>
        string HashMultiple(params object[] objects);

        /// <summary>
        /// Generates a hash string from a list of strings.
        /// Strings are joined with newline separator before hashing.
        /// </summary>
        /// <param name="strings">The list of strings to hash</param>
        /// <returns>A hash string representing all strings combined</returns>
        string HashStrings(params string[] strings);
    }
}