import { app } from "@/src/server.js";
import { connectToDatabase } from "@/src/infrastructure/database/connect.js";
import { startConsumers, stopConsumers } from "@/src/infrastructure/messaging/consumer-manager.js";
import * as logger from "@/src/shared/utils/logger.js";
import { env } from "@/src/shared/configs/env.js";

const startServer = async () => {
    try {
        await connectToDatabase();
        await startConsumers();

        const server = app.listen(env.PORT, () => {
            logger.info(`Server running on port ${env.PORT}`);
        });

        server.on("error", (err: Error) => {
            logger.error("Server error:", err);
            process.exit(1);
        });

        const shutdown = async (signal: string) => {
            logger.info(`${signal} received. Starting graceful shutdown...`);
            try {
                server.close(() => logger.info("HTTP server closed"));
                await stopConsumers();
                logger.info("Graceful shutdown completed");
                process.exit(0);
            } catch (error) {
                logger.error("Error during shutdown:", { error: String(error) });
                process.exit(1);
            }
        };

        process.on("SIGTERM", () => shutdown("SIGTERM"));
        process.on("SIGINT", () => shutdown("SIGINT"));
    } catch (error) {
        logger.error("Failed to start server:", { error: String(error) });
        process.exit(1);
    }
};

startServer();
