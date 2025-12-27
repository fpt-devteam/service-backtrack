import { AppError } from './app-error';
import HTTP_STATUS_CODES from '../constants/HTTP_STATUS_CODES';

// Export base AppError class
export { AppError };

// Specific error classes
export class BadRequestError extends AppError {
  public constructor(message: string, details?: unknown) {
    super('BadRequest', message, HTTP_STATUS_CODES.BadRequest, details);
    Object.setPrototypeOf(this, BadRequestError.prototype);
  }
}

export class NotFoundError extends AppError {
  public constructor(message: string, details?: unknown) {
    super('NotFound', message, HTTP_STATUS_CODES.NotFound, details);
    Object.setPrototypeOf(this, NotFoundError.prototype);
  }
}

export class ForbiddenError extends AppError {
  public constructor(message: string, details?: unknown) {
    super('Forbidden', message, HTTP_STATUS_CODES.Forbidden, details);
    Object.setPrototypeOf(this, ForbiddenError.prototype);
  }
}

export class UnauthorizedError extends AppError {
  public constructor(message: string, details?: unknown) {
    super('Unauthorized', message, HTTP_STATUS_CODES.Unauthorized, details);
    Object.setPrototypeOf(this, UnauthorizedError.prototype);
  }
}

export class ConflictError extends AppError {
  public constructor(message: string, details?: unknown) {
    super('Conflict', message, HTTP_STATUS_CODES.Conflict, details);
    Object.setPrototypeOf(this, ConflictError.prototype);
  }
}

export class InternalServerError extends AppError {
  public constructor(message: string, details?: unknown) {
    super(
      'InternalServerError',
      message,
      HTTP_STATUS_CODES.InternalServerError,
      details,
    );
    Object.setPrototypeOf(this, InternalServerError.prototype);
  }
}
