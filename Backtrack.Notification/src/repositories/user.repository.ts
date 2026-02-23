import { User, UserGlobalRoleType } from '@src/models/user.model';
class UserRepository {
  public async ensureExistAsync(userData: {
    _id: string,
    email?: string,
    displayName?: string,
    avatarUrl?: string | null,
    globalRole: UserGlobalRoleType,
    createdAt?: Date,
    syncedAt?: Date,
  }) {
    const existingUser = await User.findOne({ _id: userData._id, deletedAt: null });
    if (existingUser) {
      return existingUser;
    } else {
      const user = new User({
        _id: userData._id,
        email: userData.email,
        displayName: userData.displayName,
        avatarUrl: userData.avatarUrl,
        globalRole: userData.globalRole,
        createdAt: userData.createdAt ?? new Date(),
        syncedAt: userData.syncedAt ?? new Date(),
      });
      return await user.save();
    }
  }

  public async getByIdAsync(id: string) {
    return await User.findOne({ _id: id, deletedAt: null });
  }
}

export default new UserRepository();
