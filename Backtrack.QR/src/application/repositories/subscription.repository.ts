import { Subscription } from '@/src/domain/entities/subscription.entity.js';

export type SubscriptionRepository = {
  findById: (id: string) => Promise<Subscription | null>;
  findLatestByUserId: (userId: string) => Promise<Subscription | null>;
  findByProviderSubscriptionId: (providerSubscriptionId: string) => Promise<Subscription | null>;
  save: (subscription: Omit<Subscription, 'id' | 'createdAt' | 'updatedAt'>) => Promise<Subscription>;
  update: (id: string, fields: Partial<Omit<Subscription, 'id' | 'createdAt' | 'updatedAt'>>) => Promise<Subscription | null>;
};
