import { Subscription } from '@/src/domain/entities/subscription.entity.js';
import { SubscriptionDocument } from '@/src/infrastructure/database/models/subscription.model.js';

export const subscriptionToDomain = (doc: SubscriptionDocument): Subscription => ({
  id: doc._id.toString(),
  userId: doc.userId,
  providerSubscriptionId: doc.providerSubscriptionId,
  planType: doc.planType,
  status: doc.status,
  currentPeriodStart: doc.currentPeriodStart,
  currentPeriodEnd: doc.currentPeriodEnd,
  cancelAtPeriodEnd: doc.cancelAtPeriodEnd,
  createdAt: doc.createdAt,
  updatedAt: doc.updatedAt,
});
