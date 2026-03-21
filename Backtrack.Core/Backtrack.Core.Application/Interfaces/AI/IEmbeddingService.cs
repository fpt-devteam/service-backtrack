namespace Backtrack.Core.Application.Interfaces.AI
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
        Task<float[]> GenerateTextEmbeddingAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates an embedding vector from text and optional image input.
        /// Uses gemini-embedding-2-preview which maps all modalities into the same embedding space.
        /// </summary>
        Task<float[]> GenerateMultimodalEmbeddingAsync(string? text = null, string? imageBase64 = null, string? mimeType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// The dimension size of the embedding vectors produced by this service.
        /// </summary>
        int EmbeddingDimension { get; }
    }
}
