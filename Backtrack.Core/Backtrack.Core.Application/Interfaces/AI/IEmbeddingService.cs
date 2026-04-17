namespace Backtrack.Core.Application.Interfaces.AI
{
    /// <summary>
    /// Interface for generating embeddings from text.
    /// Used for semantic search and similarity matching.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generates an embedding for a search query (RETRIEVAL_QUERY task type).
        /// Must be paired with documents embedded via <see cref="GenerateDocumentEmbeddingAsync"/>.
        /// </summary>
        Task<float[]> GenerateQueryEmbeddingAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates an embedding for a document/post to be indexed (RETRIEVAL_DOCUMENT task type).
        /// Must be paired with queries embedded via <see cref="GenerateQueryEmbeddingAsync"/>.
        /// </summary>
        Task<float[]> GenerateDocumentEmbeddingAsync(string document, CancellationToken cancellationToken = default);

        /// <summary>
        /// The dimension size of the embedding vectors produced by this service.
        /// </summary>
        int EmbeddingDimension { get; }
    }
}
