namespace Backtrack.Core.Application.Exceptions.Errors;

public static class OrganizationErrors
{
    public static readonly Error NotFound = new(
        Code: "OrganizationNotFound",
        Message: "Organization not found.");

    public static readonly Error SlugAlreadyExists = new(
        Code: "SlugAlreadyExists",
        Message: "An organization with this slug already exists.");
}
