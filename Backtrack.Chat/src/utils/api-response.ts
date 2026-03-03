export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: ApiError;
  correlationId?: string;
}

/**
 * API Error structure
 */
export interface ApiError {
  code: string;
  message: string;
  details?: unknown;
}

/**
 * Helper class to build API responses
 */
export const ApiResponseBuilder = {
  success<T>(data: T, correlationId?: string): ApiResponse<T> {
    return {
      success: true,
      data,
      ...(correlationId && { correlationId }),
    };
  },

  error<T>(error: ApiError, correlationId?: string): ApiResponse<T> {
    return {
      success: false,
      error,
      ...(correlationId && { correlationId }),
    };
  },
} as const;
