namespace Backtrack.Core.Application.Usecases.Admin.GetRecentActivity;

public sealed record RecentActivityResult(
    List<RecentActivityItemResult> Data,
    int Total
);
