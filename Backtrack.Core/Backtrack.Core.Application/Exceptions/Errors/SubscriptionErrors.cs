namespace Backtrack.Core.Application.Exceptions.Errors;

public static class SubscriptionErrors
{
    public static readonly Error NotFound = new(
        Code: "SubscriptionNotFound",
        Message: "Subscription not found.");

    public static readonly Error AlreadyActive = new(
        Code: "SubscriptionAlreadyActive",
        Message: "An active subscription already exists.");

    public static readonly Error PlanNotFound = new(
        Code: "SubscriptionPlanNotFound",
        Message: "Subscription plan not found.");

    public static readonly Error PlanSubscriberTypeMismatch = new(
        Code: "SubscriptionPlanSubscriberTypeMismatch",
        Message: "The plan's subscriber type does not match the requester's subscriber type.");

    public static readonly Error CannotSubscribeToFreePlan = new(
        Code: "CannotSubscribeToFreePlan",
        Message: "Organizations cannot explicitly subscribe to a free plan tier.");

    public static readonly Error Forbidden = new(
        Code: "SubscriptionForbidden",
        Message: "You are not authorized to perform this action on this subscription.");

    public static readonly Error NotAdmin = new(
        Code: "SubscriptionNotAdmin",
        Message: "Only organization admins can manage subscriptions.");

    public static readonly Error WebhookSignatureInvalid = new(
        Code: "WebhookSignatureInvalid",
        Message: "Stripe webhook signature validation failed.");

    public static readonly Error PlanHasActiveSubscriptions = new(
        Code: "SubscriptionPlanHasActiveSubscriptions",
        Message: "Cannot archive a plan that has active subscriptions.");
}
