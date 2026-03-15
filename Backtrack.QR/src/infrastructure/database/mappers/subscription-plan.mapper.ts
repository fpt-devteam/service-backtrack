import { SubscriptionPlan } from '@/src/domain/entities/subscription-plan.entity.js';
import { SubscriptionPlanDocument } from '@/src/infrastructure/database/models/subscription-plan.model.js';

export const subscriptionPlanToDomain = (doc: SubscriptionPlanDocument): SubscriptionPlan => ({
  id: doc._id.toString(),
  name: doc.name,
  price: doc.price,
  currency: doc.currency,
  providerPriceId: doc.providerPriceId,
  features: doc.features,
  createdAt: doc.createdAt,
  updatedAt: doc.updatedAt,
});
