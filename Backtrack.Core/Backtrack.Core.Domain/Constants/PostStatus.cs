namespace Backtrack.Core.Domain.Constants
{
    public enum PostStatus
    {
        Active, // Visible and available for matching
        InStorage, // Not yet active, only visible to author/org, waiting for pickup
        ReturnScheduled,
        Returned,
        Archived,
        Expired
    }
}