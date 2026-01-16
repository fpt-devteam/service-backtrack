import mongoose from "mongoose";
import { IOrder, OrderModel, OrderStatus } from "../database/models/order.model.js";
import { createBaseRepo } from "./common/base.repository.js";

const baseRepo = createBaseRepo<IOrder, mongoose.Types.ObjectId>(OrderModel, (id) => new mongoose.Types.ObjectId(id));

const findByCode = async (code: string): Promise<IOrder | null> => {
    return await OrderModel.findOne({ code, deletedAt: null }).lean<IOrder>();
};

const updateStatus = async (orderId: mongoose.Types.ObjectId, status: OrderStatus, additionalData?: Partial<IOrder>): Promise<IOrder | null> => {
    const updateData: any = {
        status,
        ...additionalData
    };

    return await OrderModel.findOneAndUpdate(
        { _id: orderId, deletedAt: null },
        updateData,
        { new: true, runValidators: true }
    ).lean<IOrder>();
};

export const orderRepository = {
    ...baseRepo,
    findByCode,
    updateStatus
};