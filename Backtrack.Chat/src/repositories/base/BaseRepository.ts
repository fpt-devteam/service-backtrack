import type { Document, Model, UpdateQuery, FilterQuery } from 'mongoose';
import logger from '@src/utils/logger';
import ENV from '@src/common/constants/ENV';
import {
  IBaseRepository,
  PaginatedResult,
  PaginationOptions,
} from './IBaseRepository';

/**
 * Base repository implementation with common CRUD operations
 * Includes cursor-based pagination and soft delete support
 */
export abstract class BaseRepository<T extends Document>
implements IBaseRepository<T>
{
  protected model: Model<T>;

  public constructor(model: Model<T>) {
    this.model = model;
  }

  /**
   * Add soft delete filter to automatically exclude deleted items
   */
  protected addSoftDeleteFilter(
    filter: FilterQuery<T>,
  ): FilterQuery<T> {
    return { ...filter, deletedAt: null } as FilterQuery<T>;

  }

  /**
   * Find document by ID
   */
  public async findById(id: string): Promise<T | null> {
    try {
      const result = await this.model
        .findOne(this.addSoftDeleteFilter({ _id: id } as FilterQuery<T>))
        .lean();
      return result as T | null;
    } catch (error) {
      logger.error(`Error finding by ID ${id}:`);
      logger.error(error);
      throw error;
    }
  }

  /**
   * Find one document by filter
   */
  public async findOne(filter: FilterQuery<T>): Promise<T | null> {
    try {
      const result = await this.model
        .findOne(this.addSoftDeleteFilter(filter))
        .lean();
      return result as T | null;
    } catch (error) {
      logger.error('Error finding one:');
      logger.error(error);
      throw error;
    }
  }

  /**
   * Find multiple documents by filter
   */
  public async find(filter: FilterQuery<T>): Promise<T[]> {
    try {
      const results = await this.model
        .find(this.addSoftDeleteFilter(filter))
        .lean();
      return results as T[];
    } catch (error) {
      logger.error('Error finding:');
      logger.error(error);
      throw error;
    }
  }

  /**
   * Find documents with cursor-based pagination
   */
  public async findWithPagination(
    filter: FilterQuery<T>,
    options: PaginationOptions,
  ): Promise<PaginatedResult<T>> {
    try {
      const {
        cursor,
        limit = ENV.Pagination.DefaultLimit,
        sortField = '_id',
        sortOrder = 'desc',
      } = options;

      // Apply pagination limit
      const pageLimit = Math.min(
        limit,
        ENV.Pagination.MaxLimit,
      );

      // Build query with soft delete filter
      let query = this.addSoftDeleteFilter(filter);

      // Add cursor condition if provided
      if (cursor) {
        const cursorCondition =
          sortOrder === 'desc' ? { $lt: cursor } : { $gt: cursor };
        query = {
          ...query,
          [sortField]: cursorCondition,
        } as FilterQuery<T>;
      }

      // Fetch limit + 1 to determine if there are more results
      const results = await this.model
        .find(query)
        .sort({ [sortField]: sortOrder === 'desc' ? -1 : 1 })
        .limit(pageLimit + 1)
        .lean();

      // Check if there are more results
      const hasMore = results.length > pageLimit;
      const data = hasMore ? results.slice(0, pageLimit) : results;

      // Get next cursor from last item
      const nextCursor = hasMore && data.length > 0
        ? String((data[data.length - 1] as unknown as Record<string, unknown>)[sortField])
        : null;

      return {
        data: data as T[],
        nextCursor,
        hasMore,
      };
    } catch (error) {
      logger.error('Error finding with pagination:');
      logger.error(error);
      throw error;
    }
  }

  /**
   * Create a new document
   */
  public async create(data: Partial<T>): Promise<T> {
    try {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-argument
      const result = await this.model.create(data as any);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call, @typescript-eslint/no-unsafe-member-access
      return (result as any).toObject() as T;
    } catch (error) {
      logger.error('Error creating:');
      logger.error(error);
      throw error;
    }
  }

  /**
   * Create multiple documents
   */
  public async createMany(data: Partial<T>[]): Promise<T[]> {
    try {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-argument
      const results = await this.model.insertMany(data as any[]);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-call, @typescript-eslint/no-unsafe-member-access
      return results.map((r: any) => r.toObject() as T);
    } catch (error) {
      logger.error('Error creating many:');
      logger.error(error);
      throw error;
    }
  }

  /**
   * Update document by ID
   */
  public async update(
    id: string,
    data: Partial<T>,
  ): Promise<T | null> {
    try {
      const result = await this.model
        .findOneAndUpdate(
          this.addSoftDeleteFilter({ _id: id } as FilterQuery<T>),
          data as UpdateQuery<T>,
          { new: true },
        )
        .lean();

      return result as T | null;
    } catch (error) {
      logger.error(`Error updating ${id}:`);
      logger.error(error);
      throw error;
    }
  }

  /**
   * Update multiple documents
   */
  public async updateMany(
    filter: FilterQuery<T>,
    data: Partial<T>,
  ): Promise<number> {
    try {
      const result = await this.model.updateMany(
        this.addSoftDeleteFilter(filter),
        data as UpdateQuery<T>,
      );
      return result.modifiedCount;
    } catch (error) {
      logger.error('Error updating many:');
      logger.error(error);
      throw error;
    }
  }

  /**
   * Hard delete document by ID
   */
  public async delete(id: string): Promise<boolean> {
    try {
      const result = await this.model.deleteOne({ _id: id } as FilterQuery<T>);

      return result.deletedCount > 0;
    } catch (error) {
      logger.error(`Error deleting ${id}:`);
      logger.error(error);
      throw error;
    }
  }

  /**
   * Soft delete document by ID
   */
  public async softDelete(id: string): Promise<boolean> {
    try {
      const result = await this.model.updateOne(
        this.addSoftDeleteFilter({ _id: id } as FilterQuery<T>),
        { deletedAt: new Date() } as UpdateQuery<T>,
      );

      return result.modifiedCount > 0;
    } catch (error) {
      logger.error(`Error soft deleting ${id}:`);
      logger.error(error);
      throw error;
    }
  }

  /**
   * Check if document exists
   */
  public async exists(filter: FilterQuery<T>): Promise<boolean> {
    try {
      const result = await this.model.exists(
        this.addSoftDeleteFilter(filter),
      );
      return result !== null;
    } catch (error) {
      logger.error('Error checking exists:');
      logger.error(error);
      throw error;
    }
  }

  /**
   * Count documents
   */
  public async count(filter: FilterQuery<T>): Promise<number> {
    try {
      const count = await this.model.countDocuments(
        this.addSoftDeleteFilter(filter),
      );
      return count;
    } catch (error) {
      logger.error('Error counting:');
      logger.error(error);
      throw error;
    }
  }
}
