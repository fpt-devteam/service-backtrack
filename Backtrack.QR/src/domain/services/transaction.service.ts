import { CreateTransactionRequest, UpdateTransactionRequest } from "@/src/shared/contracts/transaction/transaction.request.js";
import { TransactionResponse } from "@/src/shared/contracts/transaction/transaction.response.js";
import { transactionRepository } from "@/src/infrastructure/repositories/transaction.repository.js";
import mongoose from "mongoose";
import { toTransactionResponse } from "@/src/shared/contracts/transaction/transaction.mapper.js";
import { Result, failure, success } from "@/src/shared/utils/result.js";
import { generateOrderCode } from "@/src/shared/utils/order-code.js";
export const createAsync = async (
    request: CreateTransactionRequest
): Promise<Result<TransactionResponse>> => {
    const objectOrderId = new mongoose.Types.ObjectId(request.orderId);
    const transaction = await transactionRepository.create({
        orderId: objectOrderId,
        orderCode: request.orderCode,
        paymentLinkId: request.paymentLinkId,
        amount: request.amount,
    });
    if (!transaction) {
        return failure({
            kind: "Internal",
            code: "TransactionCreationFailed",
            message: "Failed to create transaction.",
        });
    }
    return success(toTransactionResponse(transaction));
}

export const updateStatusAsync = async (
    request: UpdateTransactionRequest
): Promise<Result<{orderId: string}>> => {
    const result = await transactionRepository.updateStatus(request.paymentLinkId, request.status, {
        webhookData: request.webhookData});
    if (!result) {
        return failure({
            kind: "Internal",
            code: "TransactionUpdateFailed",
            message: "Failed to update transaction status.",
        });
    }
    return success({ orderId: result.orderId.toString() });
}
