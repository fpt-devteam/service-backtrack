using Backtrack.ApiGateway.Errors;

namespace Backtrack.ApiGateway.Exceptions;

public static class AuthErrors
{
    public static readonly Error MissingAuthHeaders = new(
        Code: "MissingAuthHeaders",
        Message: "Authorization headers are missing.");

    public static readonly Error InvalidAuthToken = new(
        Code: "InvalidAuthToken",
        Message: "The provided authentication token is invalid.");

    public static readonly Error InvalidAuthHeaderFormat = new(
        Code: "InvalidAuthHeaderFormat",
        Message: "Invalid Authorization header format. Expected: Bearer <token>");

    public static readonly Error MissingEmailInToken = new(
        Code: "MissingEmailInToken",
        Message: "Missing email in token");

    public static readonly Error EmailNotVerified = new(
        Code: "EmailNotVerified",
        Message: "Email address has not been verified.");
}