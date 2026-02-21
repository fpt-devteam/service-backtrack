import { UserRepository } from '@/src/application/repositories/user.repository.js';
import { UserModel } from '@/src/infrastructure/database/models/user.model.js';
import { userToDomain, userToPersistence } from '@/src/infrastructure/database/mappers/user.mapper.js';
import { User } from '@/src/domain/entities/user.entity.js';
import { UserEnsureExistEvent } from '@/src/infrastructure/events/user-events.js';
import { parseUserGlobalRole, UserGlobalRole } from '@/src/domain/constants/user-global-role.constant.js';

export const createUserRepository = (): UserRepository => ({
  ensureExist: async (user: UserEnsureExistEvent) => {
    const existing = await UserModel.findById(user.Id).where({ deletedAt: null });;
    if (existing) return userToDomain(existing);
    const doc = await UserModel.create(
      {
        _id: user.Id,
        email: user.Email,
        displayName: user.DisplayName,
        avatarUrl: user.AvatarUrl,
        globalRole: parseUserGlobalRole(user.GlobalRole) ?? UserGlobalRole.Customer,
        createdAt: new Date(user.CreatedAt)
      }
    );
    return userToDomain(doc);
  },
  findById: async (id: string) => {
    const doc = await UserModel.findById(id).where({ deletedAt: null });;
    return doc ? userToDomain(doc) : null;
  },

  findByEmail: async (email: string) => {
    const doc = await UserModel.findOne({ email, deletedAt: null });
    return doc ? userToDomain(doc) : null;
  },

  findByProviderCustomerId: async (providerCustomerId: string) => {
    const doc = await UserModel.findOne({ providerCustomerId, deletedAt: null });
    return doc ? userToDomain(doc) : null;
  },

  save: async (user: Omit<User, 'createdAt' | 'updatedAt'>) => {
    const doc = await UserModel.create(userToPersistence(user));
    return userToDomain(doc);
  },

  update: async (id: string, fields: Partial<Omit<User, 'createdAt' | 'updatedAt' | 'id'>>) => {
    const doc = await UserModel.findByIdAndUpdate(
      id,
      { $set: fields },
      { new: true }
    );
    return doc ? userToDomain(doc) : null;
  },

  deleteById: async (id: string) => {
    await UserModel.findByIdAndUpdate(id, { $set: { deletedAt: new Date() } });
  },
});

export const userRepository = createUserRepository();