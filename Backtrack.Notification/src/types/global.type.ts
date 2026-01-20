import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'

export type Nullable<T> = T | null

export type Optional<T> = T | undefined

export type ApiError = {
  code: string
  message: string
  details?: unknown
}

export type ApiMeta = {
  requestId?: string
  pagination?: {
    limit: number
    nextCursor?: string
  }
}

export type ApiResponse<T> =
  | { success: true; data: T }
  | { success: false; error: ApiError }
