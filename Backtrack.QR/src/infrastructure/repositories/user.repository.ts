import { IUser, UserModel, UserGlobalRoleType } from '@/src/infrastructure/database/models/user.model.js';
import { createBaseRepo } from './common/base.repository.js';

export interface UserUpsertData {
  _id: string;
  email?: string | null;
  displayName?: string | null;
  globalRole: UserGlobalRoleType;
  createdAt?: Date;
  syncedAt?: Date;
}

const userBaseRepo = createBaseRepo<IUser, string>(UserModel, (id) => id);

const upsert = async (id: string, data: UserUpsertData): Promise<IUser> => {
  const existingUser = await userBaseRepo.findById(id);

  if (existingUser) {
    // Update existing user
    const updateFields: Partial<IUser> = {
      updatedAt: new Date(),
      syncedAt: data.syncedAt || new Date()
    };

    if (data.email !== undefined) {
      updateFields.email = data.email;
    }
    if (data.displayName !== undefined) {
      updateFields.displayName = data.displayName;
    }
    updateFields.globalRole = data.globalRole;

    const updated = await UserModel
      .findOneAndUpdate(
        { _id: id, deletedAt: null },
        { $set: updateFields },
        { new: true, runValidators: true }
      )
      .lean<IUser>();

    if (!updated) {
      throw new Error(`Failed to update user ${id}`);
    }

    return updated;
  } else {
    // Create new user
    const newUser = new UserModel({
      _id: data._id,
      email: data.email,
      displayName: data.displayName,
      globalRole: data.globalRole,
      createdAt: data.createdAt || new Date(),
      syncedAt: data.syncedAt || new Date()
    });

    const saved = await newUser.save();
    return saved.toObject() as IUser;
  }
};

export const userRepository = {
  ...userBaseRepo,
  upsert
};
