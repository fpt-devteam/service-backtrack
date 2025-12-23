import { User } from '@/src/database/models/user.model.js';

export const createAsync = async (
    userData: {
        _id: string;
        email: string;
        displayName?: string;
        createdAt: Date;
        syncedAt?: Date;
    }
): Promise<InstanceType<typeof User>> => {
    const user = new User(userData);
    const saved = await user.save();
    return saved;
};

export const updateAsync = async (
    id: string,
    userData: {
        email?: string;
        displayName?: string;
        updatedAt?: Date;
        syncedAt?: Date;
    }
): Promise<InstanceType<typeof User> | null> =>
    await User.findByIdAndUpdate(
        id,
        { $set: userData },
        { new: true, runValidators: true }
    );

export const softDeleteAsync = async (
    id: string
): Promise<InstanceType<typeof User> | null> =>
    await User.findByIdAndUpdate(
        id,
        { $set: { deletedAt: new Date(), syncedAt: new Date() } },
        { new: true }
    );

export const getByIdAsync = async (
    id: string
): Promise<InstanceType<typeof User> | null> => await User.findOne({ _id: id, deletedAt: null });
