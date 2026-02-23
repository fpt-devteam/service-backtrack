import { User } from '@/src/domain/entities/user.entity.js';
import { UserEnsureExistEvent } from '@/src/infrastructure/events/user-events.js';

export type UserRepository = {
  ensureExist: (user: UserEnsureExistEvent) => Promise<User>;
  findById: (id: string) => Promise<User | null>;
  findByEmail: (email: string) => Promise<User | null>;
  findByProviderCustomerId: (providerCustomerId: string) => Promise<User | null>;
  save: (user: Omit<User, 'createdAt' | 'updatedAt'>) => Promise<User>;
  update: (id: string, fields: Partial<Omit<User, 'createdAt' | 'updatedAt' | 'id'>>) => Promise<User | null>;
  deleteById: (id: string) => Promise<void>;
};