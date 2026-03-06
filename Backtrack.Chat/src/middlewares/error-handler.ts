import { NextFunction, Request, Response } from 'express';
import { ApiResponseBuilder, ApiError } from '@/utils/api-response';
import { isAppError, type Error } from '@/utils/api-error';
import logger from '@/utils/logger';
import { ZodError } from 'zod';

const getStatusCodeFromErrorKind = (kind: Error['kind']): number => {
  switch (kind) {
    case 'Validation':
      return 400;
    case 'Unauthorized':
      return 401;
    case 'NotFound':
      return 404;
    case 'Conflict':
      return 409;
    case 'Internal':
      return 500;
    default:
      return 500;
  }
};

export const errorMiddleware = (
  err: any,
  req: Request,
  res: Response,
  _next: NextFunction
) => {
  // Log error details for debugging
  logger.error('Error occurred:', {
    name: err.name,
    error: err.message,
    stack: err.stack,
    path: req.path,
    method: req.method,
  });

  let statusCode: number;
  let errorResponse: ApiError;

  if (err instanceof ZodError) {
    // Handle Zod validation errors
    statusCode = 400;
    errorResponse = {
      code: 'VALIDATION_ERROR',
      message: 'Validation failed',
      details: err.errors.map(e => ({
        path: e.path.join('.'),
        message: e.message,
      })),
    };
  } else if (err.name === 'ValidationError' && err.errors) {
    // Handle Mongoose validation errors
    statusCode = 400;
    errorResponse = {
      code: 'VALIDATION_ERROR',
      message: 'Validation failed',
      details: Object.values(err.errors).map((e: any) => ({
        path: e.path,
        message: e.message,
      })),
    };
  } else if (isAppError(err)) {
    // Handle new Error type
    statusCode = getStatusCodeFromErrorKind(err.kind);
    errorResponse = {
      code: err.code,
      message: err.message,
    };
    if (err.cause) {
      errorResponse.details = err.cause;
    }
  } else {
    // Handle legacy errors and unknown errors
    statusCode = err.statusCode || 500;
    errorResponse = {
      code: err.code || 'INTERNAL_SERVER_ERROR',
      message: err.message || 'Something went wrong',
    };
  }

  const response = ApiResponseBuilder.error(errorResponse, req.headers['x-correlation-id'] as string);

  res.status(statusCode).json(response);
};

export default errorMiddleware;
