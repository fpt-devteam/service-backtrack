import { User } from '@/src/database/models/user.model.js';
import { Result, success, failure } from '@/src/utils/result.js';
import { Error } from '@/src/errors/error.js';

export const create = async (
    userData: {
        _id: string;
        email: string;
        displayName?: string;
        createdAt: Date;
        syncedAt?: Date;
    }
): Promise<Result<InstanceType<typeof User>>> => {
    try {
        const user = new User(userData);
        const saved = await user.save();
        return success(saved);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "CreateUserError",
            message: "Failed to create user",
            details: String(error)
        } as Error);
    }
};

export const update = async (
    id: string,
    userData: {
        email?: string;
        displayName?: string;
        updatedAt?: Date;
        syncedAt?: Date;
    }
): Promise<Result<InstanceType<typeof User> | null>> => {
    try {
        const user = await User.findByIdAndUpdate(
            id,
            { $set: userData },
            { new: true, runValidators: true }
        );
        return success(user);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "UpdateUserError",
            message: "Failed to update user",
            details: String(error)
        } as Error);
    }
};

export const softDelete = async (
    id: string
): Promise<Result<InstanceType<typeof User> | null>> => {
    try {
        const user = await User.findByIdAndUpdate(
            id,
            { $set: { deletedAt: new Date(), syncedAt: new Date() } },
            { new: true }
        );
        return success(user);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "DeleteUserError",
            message: "Failed to delete user",
            details: String(error)
        } as Error);
    }
};

export const getById = async (
    id: string
): Promise<Result<InstanceType<typeof User> | null>> => {
    try {
        const user = await User.findOne({ _id: id, deletedAt: null });
        return success(user);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "FindUserByIdError",
            message: "Failed to find user by ID",
            details: String(error)
        } as Error);
    }
};
