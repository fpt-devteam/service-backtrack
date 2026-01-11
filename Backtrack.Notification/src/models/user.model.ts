import mongoose, { Schema } from 'mongoose';

export const UserGlobalRole = {
  Customer: 'Customer',
  PlatformSuperAdmin: 'PlatformSuperAdmin',
} as const;

export type UserGlobalRoleType = 
typeof UserGlobalRole[keyof typeof UserGlobalRole];

const ROLE_VALUES = 
Object.values(UserGlobalRole) as readonly UserGlobalRoleType[];

export function parseUserGlobalRole(input: unknown): UserGlobalRoleType | null {
  if (typeof input !== 'string') return null;
  return (ROLE_VALUES as readonly string[]).includes(input) 
    ? (input as UserGlobalRoleType) : null;
}

const UserSchema = new Schema({
  _id: { type: String, required: true },
  email: { type: String, required: false, index: true },
  displayName: { type: String, default: null },
  avatarUrl: { type: String, default: null },
  globalRole: { 
    type: String, enum: Object.values(
      UserGlobalRole), default: UserGlobalRole.Customer },
  createdAt: { type: Date, required: true },
  updatedAt: { type: Date, default: null },
  deletedAt: { type: Date, default: null },
  syncedAt: { type: Date, default: Date.now },
});

export const User = mongoose.model('User', UserSchema);
