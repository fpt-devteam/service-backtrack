export const SubscriptionPlan = {
  Monthly: 'Monthly',
  Yearly: 'Yearly',
} as const;

export type SubscriptionPlanType = typeof SubscriptionPlan[keyof typeof SubscriptionPlan];

const SUBSCRIPTION_PLAN_VALUES = Object.values(SubscriptionPlan) as readonly SubscriptionPlanType[];

export function parseSubscriptionPlan(input: unknown): SubscriptionPlanType | null {
  if (typeof input !== 'string') return null;
  return (SUBSCRIPTION_PLAN_VALUES as readonly string[]).includes(input) ? (input as SubscriptionPlanType) : null;
}
