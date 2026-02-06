namespace Backtrack.Core.Application.Exceptions.Errors
{
    public static class PaginationErrors
    {
        public static readonly Error InvalidPagedQuery = new(
            Code: "InvalidPagedQuery",
            Message: "Limit and offset must be greater than 0");
    }
}
