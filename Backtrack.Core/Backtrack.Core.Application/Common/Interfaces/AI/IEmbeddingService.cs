namespace Backtrack.Core.Application.Common.Interfaces.AI
{
    /// <summary>
    /// Interface for generating embeddings from text.
    /// Used for semantic search and similarity matching.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generates an embedding vector from a single text input.
        /// </summary>
        /// <param name="text">The text to embed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A float array representing the embedding vector</returns>
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates embedding vectors from multiple text inputs in a batch.
        /// More efficient than calling GenerateEmbeddingAsync multiple times.
        /// </summary>
        /// <param name="texts">The texts to embed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of float arrays representing the embedding vectors</returns>
        Task<IList<float[]>> GenerateEmbeddingsBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the dimension size of the embedding vectors produced by this service.
        /// For example, Gemini text-embedding-004 produces 768-dimensional vectors.
        /// </summary>
        int EmbeddingDimension { get; }
    }
}
