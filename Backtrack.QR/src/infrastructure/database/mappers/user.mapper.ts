import { User } from '@/src/domain/entities/user.entity.js';
import { UserDocument } from '@/src/infrastructure/database/models/user.model.js';

export const userToDomain = (doc: UserDocument): User => ({
  id: doc._id.toString(),
  email: doc.email,
  displayName: doc.displayName,
  avatarUrl: doc.avatarUrl,
  globalRole: doc.globalRole,
  createdAt: doc.createdAt,
  updatedAt: doc.updatedAt,
  deletedAt: doc.deletedAt,
  syncedAt: doc.syncedAt,
});

export const userToPersistence = (user: Omit<User, 'createdAt' | 'updatedAt'>) => ({
  _id: user.id,
  email: user.email,
  displayName: user.displayName,
  avatarUrl: user.avatarUrl,
  globalRole: user.globalRole,
  deletedAt: user.deletedAt,
  syncedAt: user.syncedAt,
});