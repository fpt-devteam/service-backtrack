import mongoose, { Schema } from "mongoose";

export const UserGlobalRole = {
  Customer: 'Customer',
  PlatformSuperAdmin: 'PlatformSuperAdmin',
} as const;

export type UserGlobalRoleType = typeof UserGlobalRole[keyof typeof UserGlobalRole];

const ROLE_VALUES = Object.values(UserGlobalRole) as readonly UserGlobalRoleType[];
export function parseUserGlobalRole(input: unknown): UserGlobalRoleType | null {
  if (typeof input !== "string") return null;
  return (ROLE_VALUES as readonly string[]).includes(input) ? (input as UserGlobalRoleType) : null;
}

export interface IUser {
  _id: string;
  email?: string | null;
  displayName?: string | null;
  globalRole: UserGlobalRoleType;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date | null;
  syncedAt: Date;
}

const UserSchema = new Schema<IUser>(
  {
    _id: { type: String, required: true },
    email: { type: String, required: false, index: true },
    displayName: { type: String, default: null },
    globalRole: { type: String, enum: Object.values(UserGlobalRole), default: UserGlobalRole.Customer },
    deletedAt: { type: Date, default: null },
    syncedAt: { type: Date, default: Date.now },
  },
  {
    timestamps: true,
  }
);

export const UserModel = mongoose.model<IUser>('User', UserSchema);
