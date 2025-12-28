import mongoose from 'mongoose';
import { DB_NAME } from '@/src/shared/configs/constants.js';
import { env } from '@/src/shared/configs/env.js';

export const connectToDatabase = async () => {
    try {
        await mongoose.connect(env.DATABASE_URI, {
            dbName: DB_NAME,
        });
        console.log(`Connected to MongoDB database successfully. ${mongoose.connection.name}`);
    } catch (error) {
        console.error('Error connecting to MongoDB database:', error);
        process.exit(1);
    }
};