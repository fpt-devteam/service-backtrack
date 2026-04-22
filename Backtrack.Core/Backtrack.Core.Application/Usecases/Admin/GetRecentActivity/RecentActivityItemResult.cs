namespace Backtrack.Core.Application.Usecases.Admin.GetRecentActivity;

public sealed record RecentActivityItemResult(
    string PostId,
    string Title,
    string AuthorName,
    string Initials,
    string Location,
    string Status,
    DateTimeOffset CreatedAt,
    string TimeAgo
);
