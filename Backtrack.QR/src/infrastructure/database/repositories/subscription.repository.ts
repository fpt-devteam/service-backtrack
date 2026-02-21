import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { SubscriptionModel } from '@/src/infrastructure/database/models/subscription.model.js';
import { subscriptionToDomain } from '@/src/infrastructure/database/mappers/subscription.mapper.js';
import { Subscription } from '@/src/domain/entities/subscription.entity.js';

export const createSubscriptionRepository = (): SubscriptionRepository => ({
  findById: async (id: string) => {
    const doc = await SubscriptionModel.findById(id);
    return doc ? subscriptionToDomain(doc) : null;
  },

  findLatestByUserId: async (userId: string) => {
    const doc = await SubscriptionModel.findOne({ userId }).sort({ createdAt: -1 });
    return doc ? subscriptionToDomain(doc) : null;
  },

  findByProviderSubscriptionId: async (providerSubscriptionId: string) => {
    const doc = await SubscriptionModel.findOne({ providerSubscriptionId });
    return doc ? subscriptionToDomain(doc) : null;
  },

  save: async (subscription: Omit<Subscription, 'id' | 'createdAt' | 'updatedAt'>) => {
    const doc = await SubscriptionModel.create(subscription);
    return subscriptionToDomain(doc);
  },

  update: async (id: string, fields: Partial<Omit<Subscription, 'id' | 'createdAt' | 'updatedAt'>>) => {
    const doc = await SubscriptionModel.findByIdAndUpdate(
      id,
      { $set: fields },
      { new: true }
    );
    return doc ? subscriptionToDomain(doc) : null;
  },
});
