namespace Backtrack.Core.Application.Exceptions.Errors
{
    public static class UserErrors
    {
        public static readonly Error NotFound = new(
            Code: "UserNotFound",
            Message: "User not found.");

        public static readonly Error EmailAlreadyExists = new(
            Code: "EmailAlreadyExists",
            Message: "Email already exists.");

        public static readonly Error AccountNotActivated = new(
            Code: "AccountNotActivated",
            Message: "Account is existed but not activated.");

        public static readonly Error AccountAlreadyActivated = new(
            Code: "AccountAlreadyActivated",
            Message: "Account is already activated.");

        public static readonly Error NoFileUploaded = new(
            Code: "NoFileUploaded",
            Message: "No file uploaded.");
    }
}