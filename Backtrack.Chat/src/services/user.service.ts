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
		const user = await User.findByIdAndUpdate(
			userData.id,
			{
				$set: {
					email: userData.email,
					displayName: userData.displayName,
					avatarUrl: userData.avatarUrl,
					globalRole: userData.globalRole,
					providerCustomerId: userData.providerCustomerId,
					subscriptionStatus: userData.subscriptionStatus,
				},
			},
			{ upsert: true, new: true, setDefaultsOnInsert: true }
		).exec();

		logger.info(`User ${userData.id} ensured to exist`);
		return user!;
	} catch (error) {
		logger.error(`Failed to ensure user ${userData.id} exists:`, { error: String(error) });
		throw error;
	}
};
