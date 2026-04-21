import User, { IUser, UserGlobalRoleType } from '@/models/user.model';
import logger from '@/utils/logger';

export const getAllUsers = async (): Promise<IUser[]> => {
	const users = await User.find().exec();
	return users;
};

export const createUser = async (data: Partial<IUser>): Promise<IUser> => {
	const user = new User(data);
	return user.save();
};

export const getUserById = async (id: string): Promise<IUser | null> => {
	const user = await User.findById(id).exec();
	return user;
};

export const ensureUserExists = async (userData: {
	id: string;
	email?: string | null;
	displayName?: string | null;
	avatarUrl?: string | null;
	globalRole: UserGlobalRoleType;
	providerCustomerId?: string | null;
	subscriptionStatus?: string | null;
}): Promise<IUser> => {
	try {
		const fields: Record<string, unknown> = {
			displayName: userData.displayName,
			avatarUrl: userData.avatarUrl,
			globalRole: userData.globalRole,
			providerCustomerId: userData.providerCustomerId,
			subscriptionStatus: userData.subscriptionStatus,
		};

		// Only update email when it is explicitly provided — avoids overwriting a
		// valid stored email when the sync event omits the field.
		if (userData.email !== undefined) {
			fields.email = userData.email || null;
		}

		const user = await User.findByIdAndUpdate(
			userData.id,
			{ $set: fields },
			{ upsert: true, new: true, setDefaultsOnInsert: true }
		).exec();

		logger.info(`User ${userData.id} ensured to exist`);
		return user!;
	} catch (error) {
		logger.error(`Failed to ensure user ${userData.id} exists:`, { error: String(error) });
		throw error;
	}
};
