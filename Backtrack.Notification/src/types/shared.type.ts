export type Nullable<T> = T | null

export type Optional<T> = T | undefined

export type ApiError = {
  code: string
  message: string
  details?: unknown
}

export type ApiResponse<T> = { success: true; data: T } | { success: false; error: ApiError }

export type PaginatedResponse<T> = {
  items: T
  hasMore: boolean
  nextCursor: string | null
}
