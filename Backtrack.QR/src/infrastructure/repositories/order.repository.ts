import mongoose from "mongoose";
import { IOrder, OrderModel } from "../database/models/order.model.js";
import { createBaseRepo } from "./common/base.repository.js";

const baseRepo = createBaseRepo<IOrder, mongoose.Types.ObjectId>(OrderModel, (id) => new mongoose.Types.ObjectId(id));

export const orderRepository = {    
    ...baseRepo,
};