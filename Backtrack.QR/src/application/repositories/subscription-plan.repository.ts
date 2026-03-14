import { SubscriptionPlan } from '@/src/domain/entities/subscription-plan.entity.js';

export type SubscriptionPlanRepository = {
  findAll: () => Promise<SubscriptionPlan[]>;
  findByProviderPriceId: (providerPriceId: string) => Promise<SubscriptionPlan | null>;
};
