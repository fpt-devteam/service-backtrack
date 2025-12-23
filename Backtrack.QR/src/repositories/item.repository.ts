import { Item } from '@/src/database/models/item.model.js';
import { Result, success, failure } from '@/src/utils/result.js';
import { Error } from '@/src/errors/error.js';
import mongoose from 'mongoose';
import { ListItemsResult } from '../database/view/item.view.js';

export const create = async (
    item: InstanceType<typeof Item>
): Promise<Result<InstanceType<typeof Item>>> => {
    try {
        const saved = await item.save();
        return success(saved);

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
): Promise<Result<InstanceType<typeof Item> | null>> => {
    try {
        const item = await Item.findOne({
            _id: new mongoose.Types.ObjectId(id as string),
            deletedAt: null
        });
        return success(item);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "GetItemByIdError",
            message: "Failed to get item by ID",
            details: String(error)
        } as Error);
    }
};

export const getAll = async (
    ownerId: string,
    page: number,
    pageSize: number
): Promise<Result<ListItemsResult>> => {
    try {
        const skip = (page - 1) * pageSize;

        const [items, totalCount] = await Promise.all([
            Item.find({ ownerId, deletedAt: null })
                .sort({ createAt: -1 })
                .skip(skip)
                .limit(pageSize)
                .exec(),
            Item.countDocuments({ ownerId, deletedAt: null })
        ]);

        return success({ items, totalCount });
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "GetAllItemsByOwnerIdError",
            message: "Failed to get items by owner ID",
            details: String(error)
        } as Error);
    }
};
