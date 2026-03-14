import { Result, success } from '@/src/shared/core/result.js';
import { SubscriptionPlanRepository } from '@/src/application/repositories/subscription-plan.repository.js';
import { SubscriptionPlan } from '@/src/domain/entities/subscription-plan.entity.js';

type Deps = {
  subscriptionPlanRepository: SubscriptionPlanRepository;
};

const PLANS = [
  {
    name: 'Monthly',
    price: 4.99,
    currency: 'usd',
    providerPriceId: 'price_1T39LrQqedaIws156XykqIYL',
    features: [
      'Print unlimited QR codes',
      'Setup your Backtrack profile',
      'Custom QR design & branding',
    ],
  },
  {
    name: 'Yearly',
    price: 49.99,
    currency: 'usd',
    providerPriceId: 'price_1T3B5zQqedaIws15A0XEX6HR',
    features: [
      'Print unlimited QR codes',
      'Setup your Backtrack profile',
      'Custom QR design & branding',
      '2 months free',
    ],
  },
];

export type SeedSubscriptionPlansResult = {
  seeded: string[];
  skipped: string[];
};

export const seedSubscriptionPlansUseCase = (deps: Deps) => async (): Promise<Result<SeedSubscriptionPlansResult>> => {
  const seeded: string[] = [];
  const skipped: string[] = [];

  for (const plan of PLANS) {
    const existing = await deps.subscriptionPlanRepository.findByProviderPriceId(plan.providerPriceId);
    if (existing) {
      skipped.push(plan.name);
      continue;
    }

    await deps.subscriptionPlanRepository.create(plan);
    seeded.push(plan.name);
  }

  return success({ seeded, skipped });
};
