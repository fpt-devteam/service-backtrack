import type { FilterQuery } from 'mongoose';

/**
 * Pagination options for cursor-based pagination
 */
export interface PaginationOptions {
  cursor?: string; // ID of last item from previous page
  limit?: number;
  sortField?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Paginated result with cursor information
 */
export interface PaginatedResult<T> {
  data: T[];
  nextCursor: string | null;
  hasMore: boolean;
  total?: number; // Optional - expensive to compute
}

/**
 * Base repository interface for common CRUD operations
 */
export interface IBaseRepository<T> {
  // Read operations
  findById(id: string): Promise<T | null>;
  findOne(filter: FilterQuery<T>): Promise<T | null>;
  find(filter: FilterQuery<T>): Promise<T[]>;
  findWithPagination(
    filter: FilterQuery<T>,
    options: PaginationOptions,
  ): Promise<PaginatedResult<T>>;

  // Create operations
  create(data: Partial<T>): Promise<T>;
  createMany(data: Partial<T>[]): Promise<T[]>;

  // Update operations
  update(id: string, data: Partial<T>): Promise<T | null>;
  updateMany(
    filter: FilterQuery<T>,
    data: Partial<T>,
  ): Promise<number>;

  // Delete operations
  delete(id: string): Promise<boolean>;
  softDelete(id: string): Promise<boolean>;

  // Utility operations
  exists(filter: FilterQuery<T>): Promise<boolean>;
  count(filter: FilterQuery<T>): Promise<number>;
}
