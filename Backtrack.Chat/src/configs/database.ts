import mongoose from 'mongoose';
import ENV from '@src/common/constants/ENV';
import logger from 'jet-logger';

export const connectDatabase = async () => {
  const connectionString = ENV.MongodbConnectionstring;
  
  if (!connectionString) {
    throw new Error(
      'MONGODB_CONNECTIONSTRING is not defined in environment variables',
    );
  }
  
  try {
    await mongoose.connect(connectionString);
    logger.info('✅ Connected to MongoDB successfully');
  } catch (error) {
    logger.err('❌ Database connection failed:');
    logger.err(error);
    logger.warn('⚠️  Server will continue without database connection');
    logger.warn('⚠️  Check MongoDB Atlas IP whitelist or connection string');
  }
};
