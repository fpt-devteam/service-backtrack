namespace Backtrack.Core.Application.Exceptions.Errors;

public static class AdminErrors
{
    public static readonly Error Forbidden = new(
        Code: "AdminForbidden",
        Message: "Only platform super admins can perform this action.");

    public static readonly Error InvalidMonthsRange = new(
        Code: "InvalidMonthsRange",
        Message: "Months must be between 1 and 24.");
}
