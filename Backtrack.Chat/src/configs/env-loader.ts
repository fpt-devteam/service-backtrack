import logger from '@src/utils/logger';
import dotenv from 'dotenv';
import path from 'path';

const envPath = path.resolve(
  process.cwd(), '../env/backtrack-chat-api.docker.env');

const result = dotenv.config({ path: envPath });
if (result.error) {
  logger.warn(
    `Failed to load env file at ${envPath},
     falling back to default environment variables`,
  );
}
