namespace Backtrack.Core.Domain.Constants
{
    /// <summary>
    /// Represents the status of content embedding generation for a post.
    /// </summary>
    public enum ContentEmbeddingStatus
    {
        /// <summary>
        /// Embedding generation is pending and has not started yet.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Embedding is currently being generated.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Embedding has been successfully generated and is ready for use.
        /// </summary>
        Ready = 2,

        /// <summary>
        /// Embedding generation failed.
        /// </summary>
        Failed = 3
    }
}
