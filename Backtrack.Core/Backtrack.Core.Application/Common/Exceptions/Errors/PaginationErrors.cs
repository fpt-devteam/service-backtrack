namespace Backtrack.Core.Application.Common.Exceptions.Errors
{
    public static class PaginationErrors
    {
        public static readonly Error InvalidPagedQuery = new(
            Code: "InvalidPagedQuery",
            Message: "Limit and offset must be greater than 0");
    }
}
