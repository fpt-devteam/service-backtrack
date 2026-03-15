import { SubscriptionPlanRepository } from '@/src/application/repositories/subscription-plan.repository.js';
import { SubscriptionPlanModel } from '@/src/infrastructure/database/models/subscription-plan.model.js';
import { subscriptionPlanToDomain } from '@/src/infrastructure/database/mappers/subscription-plan.mapper.js';

export const createSubscriptionPlanRepository = (): SubscriptionPlanRepository => ({
  findAll: async () => {
    const docs = await SubscriptionPlanModel.find().sort({ price: 1 });
    return docs.map(subscriptionPlanToDomain);
  },

  findByProviderPriceId: async (providerPriceId: string) => {
    const doc = await SubscriptionPlanModel.findOne({ providerPriceId });
    return doc ? subscriptionPlanToDomain(doc) : null;
  },

  create: async (plan) => {
    const doc = await SubscriptionPlanModel.create(plan);
    return subscriptionPlanToDomain(doc);
  },
});
