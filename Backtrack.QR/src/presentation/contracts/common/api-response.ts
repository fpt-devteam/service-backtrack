import { Error } from "@/src/shared/core/error.js";

export type ApiSuccess<T> = { success: true; data: T; };
export type ApiFailure = { success: false; error: { code: string; message: string; details?: unknown } };

export type ApiResponse<T> = ApiSuccess<T> | ApiFailure;

export const ok = <T>(data: T): ApiSuccess<T> => ({
  success: true,
  data,
});

export const fail = (error: Error): ApiFailure => ({
  success: false,
  error: { code: error.code, message: error.message, details: error.cause },
});

const statusMap: Record<string, number> = {
  Validation: 400,
  NotFound: 404,
  Unauthorized: 401,
  Conflict: 409,
  Internal: 500
};
export const getHttpStatus = (error: Error): number => {
  return statusMap[error.kind] || 500;
};
