import { IUser, UserModel, UserGlobalRoleType } from '@/src/infrastructure/database/models/user.model.js';
import { createBaseRepo } from './common/base.repository.js';

const userBaseRepo = createBaseRepo<IUser, string>(UserModel, (id) => id);

const ensureExist = async (userData: {
  _id: string;
  email?: string | null;
  displayName?: string | null;
  avatarUrl?: string | null;
  globalRole: UserGlobalRoleType;
  createdAt?: Date;
  syncedAt?: Date;
}): Promise<IUser> => {
  const existingUser = await userBaseRepo.findById(userData._id);

  if (existingUser) {
    return existingUser;
  } else {
    const newUser = new UserModel({
      _id: userData._id,
      email: userData.email,
      displayName: userData.displayName,
      avatarUrl: userData.avatarUrl,
      globalRole: userData.globalRole,
      createdAt: userData.createdAt || new Date(),
      syncedAt: userData.syncedAt || new Date()
    });

    const saved = await newUser.save();
    return saved.toObject() as IUser;
  }
};

export const userRepository = {
  ...userBaseRepo,
  ensureExist
};
