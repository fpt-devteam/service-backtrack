import mongoose from 'mongoose';
import { env } from './environment';
import logger from '@/utils/logger';

export const connectDatabase = async (): Promise<void> => {
	try {
		logger.info(`Connecting to MongoDB: ${env.MONGODB_CONNECTIONSTRING.replace(/\/\/.*@/, '//*****@')}`);
		mongoose.set('strictQuery', true);
		await mongoose.connect(env.MONGODB_CONNECTIONSTRING);
		logger.info('MongoDB connected successfully');
	} catch (err) {
		logger.error('MongoDB connection failed', { error: (err as Error).message });
		process.exit(1);
	}
};

export const disconnectDatabase = async (): Promise<void> => {
	await mongoose.disconnect();
	logger.info('MongoDB disconnected');
};
