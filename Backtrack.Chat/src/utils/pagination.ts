import { Constants } from '@/config';
import { Model, FilterQuery } from 'mongoose';

export interface CursorPaginationParams {
  cursor?: string; // ISO date string of last item's sortField from previous page
  limit?: number;
}

export interface CursorPaginationResult<T> {
  items: T[];
  nextCursor: string | null;
  hasMore: boolean;
}

/**
 * Generic cursor-based pagination utility
 * @param model Mongoose model
 * @param filter Query filter
 * @param params Pagination params (cursor, limit)
 * @param sortField Field to sort by (default: createdAt)
 * @param sortOrder Sort order: 1 for ascending, -1 for descending (default: -1)
 * @returns Paginated result with items, nextCursor, and hasMore flag
 *
 * Cursor is the ISO date string of the last item's sortField value.
 */
export async function cursorPaginate<T extends Record<string, any>>(
  model: Model<any>,
  filter: FilterQuery<T>,
  params: CursorPaginationParams,
  sortField: string = 'createdAt',
  sortOrder: 1 | -1 = -1
): Promise<CursorPaginationResult<T>> {
  const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT); 

  const query: FilterQuery<T> = { ...filter };

  if (params.cursor) {
    const operator = sortOrder === -1 ? '$lt' : '$gt';
    (query as any)[sortField] = { [operator]: new Date(params.cursor) };
  }

  const items = await model
    .find(query)
    .sort({ [sortField]: sortOrder, _id: sortOrder })
    .limit(limit + 1)
    .lean()
    .exec();

  const hasMore = items.length > limit;

  if (hasMore) {
    items.pop();
  }

  const lastItem = items[items.length - 1] as any;
  const cursorValue = lastItem?.[sortField];
  const nextCursor =
    hasMore && items.length > 0
      ? cursorValue instanceof Date
        ? cursorValue.toISOString()
        : String(cursorValue)
      : null;

  return {
    items: items as T[],
    nextCursor,
    hasMore,
  };
}

export const buildPaginatedResult = <T>(
    results: T[],
    limit: number,
    cursorField: keyof T
): CursorPaginationResult<T> => {
    const hasMore = results.length > limit;
    if (hasMore) results.pop();

    const last = results[results.length - 1] as any;
    const cursorValue = last?.[cursorField];
    const nextCursor = hasMore && cursorValue
        ? cursorValue instanceof Date
            ? cursorValue.toISOString()
            : String(cursorValue)
        : null;

    return { items: results, nextCursor, hasMore };
};

export const buildCursorStage = (cursor?: string, sortOrder: 1 | -1 = -1) => {
    if (!cursor) return {};
    const operator = sortOrder === -1 ? '$lt' : '$gt';
    return { [operator]: new Date(cursor) };
};