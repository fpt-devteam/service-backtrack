import mongoose from 'mongoose';
import ENV from '@src/common/constants/ENV';
import logger from '@src/utils/logger';

export const connectDatabase = async () => {
  const connectionString = ENV.MongodbConnectionstring;


  if (!connectionString) {
    throw new Error(
      'MONGODB_CONNECTIONSTRING is not defined in environment variables',
    );
  }
  logger.warn(` Connecting to MongoDB... ${connectionString}`);

  try {
    await mongoose.connect(connectionString);
    logger.info('✅ Connected to MongoDB successfully');
  } catch (error) {
    logger.error('❌ Database connection failed:');
    logger.error(error);
    logger.error(error instanceof Error ? error.message : String(error));
    logger.warn('⚠️  Server will continue without database connection');
    logger.warn('⚠️  Check MongoDB Atlas IP whitelist or connection string');
  }
};
