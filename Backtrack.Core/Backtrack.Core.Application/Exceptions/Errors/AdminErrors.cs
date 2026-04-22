namespace Backtrack.Core.Application.Exceptions.Errors;

public static class AdminErrors
{
    public static readonly Error Forbidden = new(
        Code: "AdminForbidden",
        Message: "Only platform super admins can perform this action.");

    public static readonly Error InvalidMonthsRange = new(
        Code: "InvalidMonthsRange",
        Message: "Months must be between 1 and 24.");

    public static readonly Error InvalidLimitRange = new(
        Code: "InvalidLimitRange",
        Message: "Limit must be between 1 and 50.");

    public static readonly Error InvalidPostStatus = new(
        Code: "InvalidPostStatus",
        Message: "Status must be one of: Active, InStorage, Returned, Archived, Expired.");

    public static readonly Error InvalidPage = new(
        Code: "InvalidPage",
        Message: "Page must be at least 1.");

    public static readonly Error InvalidPageSize = new(
        Code: "InvalidPageSize",
        Message: "PageSize must not exceed 50.");

    public static readonly Error InvalidOrgStatus = new(
        Code: "InvalidOrgStatus",
        Message: "Status must be one of: Active, Expired, Inactive.");
}
