import type { CreateItemRequest } from '@/src/contracts/item/request.js';
import type { ItemResponse } from '@/src/contracts/item/response.js';
import type { CreateItemInput, ItemDocument } from '@/src/repositories/item.repository.js';

/**
 * Request -> Repository Input
 */
export const toCreateItemInput = (request: CreateItemRequest, userId: string): CreateItemInput => ({
    name: request.name,
    description: request.description,
    imageUrls: request.imageUrls,
    userId
});

/**
 * Repository Document -> Response
 */
export const toItemResponse = (doc: ItemDocument): ItemResponse => ({
    id: doc._id.toString(),
    name: doc.name,
    description: doc.description,
    imageUrls: doc.imageUrls,
    userId: doc.userId,
    createdAt: doc.createAt.toISOString(),
    updatedAt: doc.updateAt?.toISOString() || null
});
