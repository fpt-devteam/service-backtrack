namespace Backtrack.Core.Domain.Constants;

public enum PostMatchingStatus
{
    /// <summary>Post created or content changed — waiting for embedding to complete before matching.</summary>
    Pending = 0,

    /// <summary>Matching is actively running.</summary>
    Processing = 1,

    /// <summary>Matching completed successfully (may have zero or more matches).</summary>
    Completed = 2,

    /// <summary>Matching failed. Hangfire will retry.</summary>
    Failed = 3
}
