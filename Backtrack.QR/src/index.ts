import dotenv from 'dotenv';
import { app } from '@/src/server.js';
import { connectToDatabase } from '@/src/database/connect.js';

dotenv.config({
    path: './backtrack-qr.local.env'
});

const startServer = async () => {
    try {
        await connectToDatabase();

        const server = app.listen(process.env.PORT, () => {
            console.log(`Server running on port ${process.env.PORT}`);
        });

        server.on("error", (err: Error) => {
            console.error("Server error:", err);
            process.exit(1);
        });

    } catch (error) {
        console.error("Failed to start server:", error);
        process.exit(1);
    }
};

startServer();
