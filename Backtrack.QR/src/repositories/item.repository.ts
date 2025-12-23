import { Item } from '@/src/database/models/item.model.js';
import mongoose from 'mongoose';
import type { ClientSession } from "mongoose";
import { ListItemsResult } from '../database/view/item.view.js';

export const createAsync = async (
    item: InstanceType<typeof Item>,
    session?: ClientSession
): Promise<InstanceType<typeof Item>> => await item.save({ session });

export const getByIdAsync = async (
    id: mongoose.Types.ObjectId
): Promise<InstanceType<typeof Item> | null> => await Item.findOne({
    _id: id,
    deletedAt: null
});

export const getAllAsync = async (
    ownerId: string,
    page: number,
    pageSize: number
): Promise<ListItemsResult> => {
    const skip = (page - 1) * pageSize;
    const [items, totalCount] = await Promise.all([
        Item.find({ ownerId, deletedAt: null })
            .sort({ createAt: -1 })
            .skip(skip)
            .limit(pageSize)
            .exec(),
        Item.countDocuments({ ownerId, deletedAt: null })
    ]);

    return { items, totalCount };

};
