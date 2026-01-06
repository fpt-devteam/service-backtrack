import mongoose from "mongoose";
import { IPayment, PaymentModel } from "../database/models/payment.model.js";
import { createBaseRepo } from "./common/base.repository.js";


const baseRepo = createBaseRepo<IPayment, mongoose.Types.ObjectId>(PaymentModel, (id) => new mongoose.Types.ObjectId(id));

export const paymentRepository = {    
    ...baseRepo,
};