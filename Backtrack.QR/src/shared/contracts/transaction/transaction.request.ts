import { TransactionStatus } from "@/src/infrastructure/database/models/transaction.model.js";
import { WebhookData } from "@payos/node";

export type CreateTransactionRequest = {
    orderId: string;
    paymentLinkId: string;
    orderCode: number;
    amount: number;
}

export type UpdateTransactionRequest = {
    paymentLinkId: string;
    status: TransactionStatus;
    webhookData?: WebhookData;
}