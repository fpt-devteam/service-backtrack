using Backtrack.ApiGateway.Errors;
using System.Net;

namespace Backtrack.Core.Application.Exceptions;

public static class AuthErrors
{
    public static readonly Error MissingAuthHeaders = new(
        Code: "MissingAuthHeaders",
        Message: "Authorization headers are missing.",
        HttpStatusCode.Unauthorized);

    public static readonly Error InvalidAuthToken = new(
        Code: "InvalidAuthToken",
        Message: "The provided authentication token is invalid.",
        HttpStatusCode.Unauthorized);

    public static readonly Error InvalidAuthHeaderFormat = new(
        Code: "InvalidAuthHeaderFormat",
        Message: "Invalid Authorization header format. Expected: Bearer <token>",
        HttpStatusCode.Unauthorized);

    public static readonly Error MissingEmailInToken = new(
        Code: "MissingEmailInToken",
        Message: "Missing email in token",
        HttpStatusCode.Unauthorized);
}