using System.Net;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public static class PostErrors
    {
        public static readonly Error InvalidPostType = new(
            Code: "InvalidPostType",
            Message: "PostType must be either 'Lost' or 'Found'.",
            HttpStatusCode.BadRequest);

        public static readonly Error NotFound = new(
            Code: "PostNotFound",
            Message: "Post not found.",
            HttpStatusCode.NotFound);
    }
}
