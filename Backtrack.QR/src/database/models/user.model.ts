import mongoose, { HydratedDocument, Schema } from "mongoose";

export interface IUser {
    _id: string;
    email: string;
    displayName?: string | null;
    createdAt: Date;
    updatedAt: Date;
    deletedAt?: Date | null;
    syncedAt: Date;
}

export type UserDocument = HydratedDocument<IUser>;

const UserSchema = new Schema<IUser>(
    {
        _id: { type: String, required: true },
        email: { type: String, required: true, index: true },
        displayName: { type: String, default: null },
        deletedAt: { type: Date, default: null },
        syncedAt: { type: Date, default: Date.now },
    },
    {
        timestamps: true, // Automatically adds createdAt and updatedAt
    }
);

export const User = mongoose.model<IUser>('User', UserSchema);
