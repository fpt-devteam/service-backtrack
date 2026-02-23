import mongoose from 'mongoose';
import { env } from '@/src/infrastructure/configs/env.js';
import * as logger from '@/src/shared/core/logger.js';

const DB_NAME = 'backtrack-qr-db';
export const connectToDatabase = async () => {
  try {
    await mongoose.connect(env.DATABASE_URI, {
      dbName: DB_NAME,
    });
    logger.info('Connected to MongoDB database successfully', { databaseUri: env.DATABASE_URI });
  } catch (error) {
    logger.error('Error connecting to MongoDB database:', error as Object);
    process.exit(1);
  }
};