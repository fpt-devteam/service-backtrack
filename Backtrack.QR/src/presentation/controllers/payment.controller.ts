import { TransactionStatus } from "@/src/infrastructure/database/models/transaction.model.js";
import * as transactionService from "@/src/domain/services/transaction.service.js";
import * as orderService from "@/src/domain/services/order.service.js";
import * as paymentService from "@/src/domain/services/payment.service.js";
import { isFailure } from "@/src/shared/utils/result.js";
import { Request, Response } from 'express';
import { HEADER_AUTH_ID } from "@/src/shared/utils/headers.js";

export const reCreateLinkPaymentAsync = async (request: Request, response: Response): Promise<void> => {
    const { orderId } = request.params;
    const correlationId = (request as any).correlationId || 'unknown';
    const ownerId = request.headers[HEADER_AUTH_ID] as string;
    const orderResult = await orderService.findByIdAsync(orderId);
    if (isFailure(orderResult)) {
        response.status(500).json({
            error: orderResult.error,

            correlationId
        });
        return;
    }
    const order = orderResult.value;
    const paymentResult = await paymentService.createLinkPaymentAsync(
        order.code,
        order.packageSnapshot.price,
        ownerId
    );
    if (isFailure(paymentResult)) {
        response.status(500).json({
            error: paymentResult.error,
            correlationId
        });
        return;
    }
    await transactionService.createAsync({
        orderId: order.id,
        orderCode: paymentResult.value.orderCode,
        paymentLinkId: paymentResult.value.paymentLinkId,
        amount: order.packageSnapshot.price,
    });
    response.status(201).json({
        order,
        paymentLink: paymentResult.value
    });
}

export const paymentFailedAsync = async (request: Request, response: Response): Promise<void> => {
    const { id } = request.query;
    await transactionService.updateStatusAsync({
        paymentLinkId: id as string,
        status: TransactionStatus.FAILED,
    });
    response.status(200).json({ message: 'Payment failed recorded' });
}

export const paymentSucceedAsync = async (request: Request, response: Response): Promise<void> => {
    const { id } = request.query;
    const transactionResult = await transactionService.updateStatusAsync({
        paymentLinkId: id as string,
        status: TransactionStatus.SUCCESS,
    });
    if (isFailure(transactionResult)) {
        response.status(500).json({ error: transactionResult.error });
        return;
    }
    response.status(200).json({ message: 'Payment succeed recorded' });
}