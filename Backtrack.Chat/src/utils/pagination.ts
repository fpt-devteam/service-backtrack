import { Model, Document, FilterQuery } from 'mongoose';

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
export async function cursorPaginate<T extends Document>(
  model: Model<T>,
  filter: FilterQuery<T>,
  params: CursorPaginationParams,
  sortField: string = 'createdAt',
  sortOrder: 1 | -1 = -1
): Promise<CursorPaginationResult<T>> {
  const limit = Math.min(params.limit || 20, 100); // Max 100 items per page

  const query: FilterQuery<T> = { ...filter };

  // Use sortField (e.g. createdAt) for cursor comparison, not _id
  if (params.cursor) {
    const operator = sortOrder === -1 ? '$lt' : '$gt';
    (query as any)[sortField] = { [operator]: new Date(params.cursor) };
  }

  // Fetch limit + 1 to check if there are more items
  const items = await model
    .find(query)
    .sort({ [sortField]: sortOrder, _id: sortOrder })
    .limit(limit + 1)
    .lean()
    .exec();

  // Check if there are more items
  const hasMore = items.length > limit;

  // Remove the extra item
  if (hasMore) {
    items.pop();
  }

  // Get next cursor: ISO string of the sortField value of the last item
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
