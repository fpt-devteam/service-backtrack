export const StripeSubscriptionStatus = {
  ACTIVE: "active",
  INCOMPLETE: "incomplete",
  INCOMPLETE_EXPIRED: "incomplete_expired",
  PAST_DUE: "past_due",
  CANCELED: "canceled",
  UNPAID: "unpaid",
} as const;

export const ONGOING_SUBSCRIPTION_STATUSES: string[] = [
  StripeSubscriptionStatus.ACTIVE,
  StripeSubscriptionStatus.PAST_DUE,
  StripeSubscriptionStatus.UNPAID,
];

export const TERMINATED_SUBSCRIPTION_STATUSES: string[] = [
  StripeSubscriptionStatus.CANCELED,
  StripeSubscriptionStatus.INCOMPLETE_EXPIRED,
];

export const PENDING_PAYMENT_STATUSES: string[] = [
  StripeSubscriptionStatus.INCOMPLETE,
];
