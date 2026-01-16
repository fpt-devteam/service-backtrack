import type { Error } from '@/src/shared/errors/error.js';

const ERROR_KIND_TO_STATUS: Record<string, number> = {
    NotFound: 404,
    Conflict: 409,
    Validation: 400,
    Unauthorized: 401,
    Forbidden: 403,
    Internal: 500,
};

export const getHttpStatusCode = (error: Error): number => {
    return ERROR_KIND_TO_STATUS[error.kind] ?? 500;
};
