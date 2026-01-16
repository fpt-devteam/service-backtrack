import mongoose from "mongoose";
import { ITransaction, TransactionModel, TransactionStatus } from "../database/models/transaction.model.js";
import { createBaseRepo } from "./common/base.repository.js";


const baseRepo = createBaseRepo<ITransaction, mongoose.Types.ObjectId>(TransactionModel, (id) => new mongoose.Types.ObjectId(id));

const getByOrderCode = async (orderCode: number): Promise<ITransaction | null> => {
    return await TransactionModel.findOne({ orderCode, deletedAt: null }).lean<ITransaction>();
}
const updateStatus = async (paymentLinkId: string, status: TransactionStatus, additionalData?: Partial<ITransaction>): Promise<ITransaction | null> => {
    const updateData: any = {
        status,
        ...additionalData
    };

    return await TransactionModel.findOneAndUpdate(
        { paymentLinkId, deletedAt: null },
        updateData,
        { new: true, runValidators: true }
    ).lean<ITransaction>();
};

export const transactionRepository = {    
    ...baseRepo,
    updateStatus,
    getByOrderCode,
};