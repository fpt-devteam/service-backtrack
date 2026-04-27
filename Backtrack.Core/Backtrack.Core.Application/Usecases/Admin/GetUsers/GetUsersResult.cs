namespace Backtrack.Core.Application.Usecases.Admin.GetUsers;

public sealed record GetUsersResult
{
    public required List<AdminUserSummaryResult> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
    /// <summary>Total real users (with email) across all pages, matching current filters.</summary>
    public required int RealUserCount { get; init; }
    /// <summary>Total anonymous users (no email) in the system, unaffected by filters.</summary>
    public required int AnonymousCount { get; init; }
}
