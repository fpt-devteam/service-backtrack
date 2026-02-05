namespace Backtrack.Core.Application.Exceptions.Errors
{
    public static class PostErrors
    {
        public static readonly Error InvalidPostType = new(
            Code: "InvalidPostType",
            Message: "PostType must be either 'Lost' or 'Found'.");

        public static readonly Error NotFound = new(
            Code: "PostNotFound",
            Message: "Post not found.");
    }
}
