namespace Backtrack.Core.Application.Exceptions.Errors;

public static class OrganizationInventoryErrors
{
    public static Error MissingRequiredFinderContactField(string fieldName) => new(
        Code: "MissingRequiredFinderContactField",
        Message: $"This organization requires finder contact field '{fieldName}'.");
}
