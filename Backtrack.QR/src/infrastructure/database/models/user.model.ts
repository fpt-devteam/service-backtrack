import { Schema, model, Document } from 'mongoose';
import { UserGlobalRoleType, UserGlobalRole } from '@/src/domain/constants/user-global-role.constant.js';

export interface UserDocument extends Document<string> {
  _id: string;
  email?: string | null;
  displayName?: string | null;
  avatarUrl?: string | null;
  globalRole: UserGlobalRoleType;
  providerCustomerId?: string | null;
  subscriptionStatus?: string | null;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date | null;
}

const userSchema = new Schema<UserDocument>(
  {
    _id: { type: String, required: true },
    email: { type: String, default: null },
    displayName: { type: String, default: null },
    avatarUrl: { type: String, default: null },
    globalRole: {
      type: String,
      enum: Object.values(UserGlobalRole),
      required: true,
    },
    providerCustomerId: { type: String, default: null, index: true },
    subscriptionStatus: { type: String, default: null },
    deletedAt: { type: Date, default: null },
  },
  {
    timestamps: true,
  }
);

export const UserModel = model<UserDocument>('User', userSchema);