import { ITransaction } from "@/src/infrastructure/database/models/transaction.model.js";
import { TransactionResponse } from "./transaction.response.js";
import { toVietnamISOStringOrDefault } from "@/src/shared/utils/timezone.js";

export const toTransactionResponse = (transaction: ITransaction): TransactionResponse => {
    return {
        orderId: transaction.orderId.toHexString(),
        paymentLinkId: transaction.paymentLinkId,
        amount: transaction.amount,
        status: transaction.status,
        createdAt: toVietnamISOStringOrDefault(transaction.createdAt),
        updatedAt: toVietnamISOStringOrDefault(transaction.updatedAt),
    };
}