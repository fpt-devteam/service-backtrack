import type { Error } from '@/src/shared/errors/error.js';

export type Result<T> = { success: true; value: T } | { success: false; error: Error };

export const success = <T>(value: T): Result<T> => ({ success: true, value });
export const failure = (error: Error): Result<never> => ({ success: false, error });

export const isSuccess = <T>(result: Result<T>): result is { success: true; value: T } =>
    result.success === true;

export const isFailure = <T>(result: Result<T>): result is { success: false; error: Error } =>
    result.success === false;
