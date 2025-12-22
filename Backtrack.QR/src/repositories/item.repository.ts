import { Item } from '@/src/database/models/item.model.js';
import { Result, success, failure } from '@/src/utils/result.js';
import { Error } from '@/src/errors/error.js';
import mongoose from 'mongoose';
import * as logger from '@/src/utils/logger.js';

export type ItemDocument = {
    _id: mongoose.Types.ObjectId;
    name: string;
    description?: string;
    imageUrls?: string[];
    userId: string;
    createAt: Date;
    updateAt: Date | null;
    deletedAt: Date | null;
};

export type CreateItemInput = {
    name: string;
    description?: string;
    imageUrls?: string[];
    userId: string;
};

export const create = async (
    input: CreateItemInput
): Promise<Result<ItemDocument>> => {
    try {
        const item = new Item({
            ...input
        });
        const saved = await item.save();
        return success(saved.toObject() as ItemDocument);

    } catch (error) {
        return failure({
            kind: "Internal",
            code: "CreateItemError",
            message: "Failed to create item",
            details: String(error)
        } as Error);
    }
};

export const getById = async (
    id: string | mongoose.Types.ObjectId
): Promise<Result<ItemDocument | null>> => {
    try {
        const item = await Item.findOne({
            _id: new mongoose.Types.ObjectId(id as string),
            deletedAt: null
        });
        return success(item ? (item.toObject() as ItemDocument) : null);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "GetItemByIdError",
            message: "Failed to get item by ID",
            details: String(error)
        } as Error);
    }
};
