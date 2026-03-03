import { Schema, model } from 'mongoose';

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
	avatarUrl?: string | null;
	globalRole: UserGlobalRoleType;
	providerCustomerId?: string | null;
	subscriptionStatus?: string | null;
	createdAt: Date;
	updatedAt: Date;
	deletedAt?: Date | null;
}

const userSchema = new Schema<IUser>(
	{
		_id: { type: String, required: true },
		email: { type: String, default: null, unique: true, sparse: true },
		displayName: { type: String, default: null },
		avatarUrl: { type: String, default: null },
		globalRole: { 
			type: String, 
			enum: Object.values(UserGlobalRole),
			default: UserGlobalRole.Customer,
			required: true
		},
		providerCustomerId: { type: String, default: null },
		subscriptionStatus: { type: String, default: null },
		deletedAt: { type: Date, default: null },
	},
	{ timestamps: true },
);

const User = model<IUser>('User', userSchema);
export default User;
