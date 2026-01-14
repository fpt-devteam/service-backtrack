import mongoose from "mongoose";
import { ITransaction, TransactionModel } from "../database/models/transaction.model.js";
import { createBaseRepo } from "./common/base.repository.js";


const baseRepo = createBaseRepo<ITransaction, mongoose.Types.ObjectId>(TransactionModel, (id) => new mongoose.Types.ObjectId(id));
export const paymentRepository = {    
    ...baseRepo,
};