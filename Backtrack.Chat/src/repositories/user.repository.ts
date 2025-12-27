import { User } from '@src/models/user.model';

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
