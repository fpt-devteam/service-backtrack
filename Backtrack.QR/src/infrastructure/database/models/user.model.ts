import mongoose, { Schema } from "mongoose";

export interface IUser {
    _id: string;
    email: string;
    displayName?: string | null;
    createdAt: Date;
    updatedAt: Date;
    deletedAt?: Date | null;
    syncedAt: Date;
}

const UserSchema = new Schema<IUser>(
    {
        _id: { type: String, required: true },
        email: { type: String, required: true, index: true },
        displayName: { type: String, default: null },
        deletedAt: { type: Date, default: null },
        syncedAt: { type: Date, default: Date.now },
    },
    {
        timestamps: true,
    }
);

export const UserModel = mongoose.model<IUser>('User', UserSchema);
