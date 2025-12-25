import dotenv from 'dotenv';
import path from 'path';

// Load environment variables from centralized env directory
const envPath = path.resolve(process.cwd(), '../env/backtrack-chat-api.docker.env');

const result = dotenv.config({ path: envPath });

// Log warning only in development
if (result.error && process.env.NODE_ENV === 'development') {
  console.log(`Note: Environment file not found at ${envPath}`);
  console.log('Using system environment variables or defaults');
}
