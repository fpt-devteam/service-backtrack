import { User } from '@/src/domain/entities/user.entity.js';
import { UserDocument } from '@/src/infrastructure/database/models/user.model.js';

export const userToDomain = (doc: UserDocument): User => ({
  id: doc._id.toString(),
  email: doc.email,
  displayName: doc.displayName,
  avatarUrl: doc.avatarUrl,
  globalRole: doc.globalRole,
  providerCustomerId: doc.providerCustomerId,
  subscriptionStatus: doc.subscriptionStatus,
  createdAt: doc.createdAt,
  updatedAt: doc.updatedAt,
  deletedAt: doc.deletedAt,
});

export const userToPersistence = (user: Omit<User, 'createdAt' | 'updatedAt'>) => ({
  _id: user.id,
  email: user.email,
  displayName: user.displayName,
  avatarUrl: user.avatarUrl,
  globalRole: user.globalRole,
  providerCustomerId: user.providerCustomerId,
  subscriptionStatus: user.subscriptionStatus,
  deletedAt: user.deletedAt,
});