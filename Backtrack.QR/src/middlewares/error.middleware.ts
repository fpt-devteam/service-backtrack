import { Request, Response, NextFunction } from 'express';
import { Error } from '@/src/errors/error.js';

export const errorMiddleware = (
    err: Error,
    req: Request,
    res: Response,
    _next: NextFunction
) => {
    const correlationId = req.correlationId || 'unknown';
    const statusCode = 500;
    const errorCode = err.code || 'InternalServerError';
    const message = err.message || 'An unexpected error occurred';
    const details = err.details;

    console.error(`[${correlationId}] Error:`, err);

    res.status(statusCode).json({
        success: false,
        error: {
            code: errorCode,
            message: message,
            details: details
        },
        correlationId
    });
};
