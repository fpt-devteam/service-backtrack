namespace Backtrack.Core.Application.Usecases.Subscriptions.CreateCustomerPortalSession;

public sealed record CreateCustomerPortalSessionResult
{
    public required string Url { get; init; }
}
