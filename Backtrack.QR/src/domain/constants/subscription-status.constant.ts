export const SubscriptionStatus = {
  Active: 'Active',
  PastDue: 'PastDue',
  Canceled: 'Canceled',
} as const;

export type SubscriptionStatusType = typeof SubscriptionStatus[keyof typeof SubscriptionStatus];

const SUBSCRIPTION_STATUS_VALUES = Object.values(SubscriptionStatus) as readonly SubscriptionStatusType[];

export function parseSubscriptionStatus(input: unknown): SubscriptionStatusType | null {
  if (typeof input !== 'string') return null;
  return (SUBSCRIPTION_STATUS_VALUES as readonly string[]).includes(input) ? (input as SubscriptionStatusType) : null;
}
