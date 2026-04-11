namespace Backtrack.Core.Domain.Constants;

public enum SubscriptionStatus
{
    Active,
    PastDue,
    Unpaid,
    Incomplete,
    IncompleteExpired,
    Canceled
}
