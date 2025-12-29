import { User, UserGlobalRoleType } from '@src/models/user.model';

class UserRepository {
  /**
   * Create a new user
   */
  public async createAsync(userData: {
    _id: string,
    email: string,
    displayName?: string,
    avatarUrl?: string | null,
    createdAt: Date,
    syncedAt?: Date,
  }) {
    const user = new User(userData);
    const saved = await user.save();
    return saved;
  }

  /**
   * Update user
   */
  public async updateAsync(
    id: string,
    userData: {
      email?: string,
      displayName?: string,
      avatarUrl?: string | null,
      updatedAt?: Date,
      syncedAt?: Date,
    },
  ) {
    return await User.findByIdAndUpdate(
      id,
      { $set: userData },
      { new: true, runValidators: true },
    );
  }

  /**
   * Upsert user (create if not exists, update if exists)
   */
  public async upsertAsync(userData: {
    _id: string,
    email?: string,
    displayName?: string,
    avatarUrl?: string | null,
    globalRole: UserGlobalRoleType,
    createdAt?: Date,
    syncedAt?: Date,
  }) {
    const existingUser = await this.getByIdAsync(userData._id);

    if (existingUser) {
      // Update existing user
      return await User.findByIdAndUpdate(
        userData._id,
        {
          $set: {
            ...(userData.email !== undefined && { email: userData.email }),
            ...(userData.displayName !== undefined && 
              { displayName: userData.displayName }),
            ...(userData.avatarUrl !== undefined && 
              { avatarUrl: userData.avatarUrl }),
            ...(userData.globalRole !== undefined && 
              { globalRole: userData.globalRole }),
            updatedAt: new Date(),
            syncedAt: userData.syncedAt ?? new Date(),
          },
        },
        { new: true, runValidators: true },
      );
    } else {
      // Create new user
      const user = new User({
        _id: userData._id,
        email: userData.email,
        displayName: userData.displayName,
        avatarUrl: userData.avatarUrl,
        // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
        globalRole: userData.globalRole,
        createdAt: userData.createdAt ?? new Date(),
        syncedAt: userData.syncedAt ?? new Date(),
      });
      return await user.save();
    }
  }

  /**
   * Soft delete user
   */
  public async softDeleteAsync(id: string) {
    return await User.findByIdAndUpdate(
      id,
      { $set: { deletedAt: new Date(), syncedAt: new Date() } },
      { new: true },
    );
  }

  /**
   * Find user by ID (excluding deleted)
   */
  public async getByIdAsync(id: string) {
    return await User.findOne({ _id: id, deletedAt: null });
  }

  /**
   * Find multiple users by IDs
   */
  public async findByIds(userIds: string[]) {
    return await User.find({ _id: { $in: userIds }, deletedAt: null }).exec();
  }

  /**
   * Check if user exists
   */
  public async exists(userId: string): Promise<boolean> {
    const count = await User.countDocuments({
      _id: userId,
      deletedAt: null,
    }).exec();
    return count > 0;
  }
}

export default new UserRepository();
