import type { Request, Response, NextFunction } from "express";
import * as logger from "@/src/utils/logger.js";

const getCorrelationId = (req: Request) =>
    (req as any).correlationId || "unknown";

export const errorMiddleware = (
    err: unknown,
    req: Request,
    res: Response,
    _next: NextFunction
) => {
    const correlationId = getCorrelationId(req);

    if (err instanceof globalThis.Error) {
        logger.error(`[${correlationId}] Unhandled exception`, {
            route: req.originalUrl,
            method: req.method,
            name: err.name,
            message: err.message,
            stack: err.stack,
        });
    } else {
        logger.error(`[${correlationId}] Unhandled thrown value`, {
            route: req.originalUrl,
            method: req.method,
            err,
        });
    }

    return res.status(500).json({
        success: false,
        error: {
            code: "InternalServerError",
            message: "An unexpected error occurred",
        },
        correlationId,
    });
};
