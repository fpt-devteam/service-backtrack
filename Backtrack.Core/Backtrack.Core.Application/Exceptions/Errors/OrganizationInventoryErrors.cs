namespace Backtrack.Core.Application.Exceptions.Errors;

public static class OrganizationInventoryErrors
{
    public static Error MissingRequiredOrgContractField(string fieldName) => new(
        Code: "MissingRequiredOrgContractField",
        Message: $"This organization requires finder contact field '{fieldName}'.");
}
