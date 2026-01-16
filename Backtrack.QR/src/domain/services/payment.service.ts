import { env } from "@/src/shared/configs/env.js";
import { paymentAdapter, PaymentStatus } from "@/src/infrastructure/payment/payos.adapter.js";
import { Result, success, failure } from "@/src/shared/utils/result.js";
import { PaymentLinkResponse } from "@/src/infrastructure/payment/dtos/payment.responses.js";
import * as logger from "@/src/shared/utils/logger.js";
import { Webhook } from "@payos/node";
import { orderRepository } from "@/src/infrastructure/repositories/order.repository.js";
import { OrderStatus } from "@/src/infrastructure/database/models/order.model.js";
import { publishMessage } from "@/src/infrastructure/messaging/rabbitmq-publisher.js";
import { EventTopics } from "@/src/shared/contracts/events/event-topics.js";
import type { QrGenerationRequestedEvent } from "@/src/shared/contracts/events/order-events.js";
import { transactionRepository } from "@/src/infrastructure/repositories/transaction.repository.js";
import { generateOrderCode } from "@/src/shared/utils/order-code.js";

export const createLinkPaymentAsync = async (orderCodeSystem: string, amount: number, userId: string): Promise<Result<PaymentLinkResponse>> => {
    try {
        const orderCode = generateOrderCode(userId)
        const result = await paymentAdapter.createPaymentLink({
            orderCode,
            amount,
            description: `${orderCodeSystem}`,
            returnUrl: `${env.SERVICE_BACKTRACK}/${env.SUCCESS_ENDPOINT}`,
            cancelUrl: `${env.SERVICE_BACKTRACK}/${env.CANCEL_ENDPOINT}`
        });
        return success(result);
    } catch (error) {
        return failure({
            kind: 'Internal',
            code: 'PAYMENT_LINK_CREATION_FAILED',
            message: error instanceof Error ? error.message : 'Failed to create payment link',
            cause: error
        });
    }
}

const handlePaymentSuccessAsync = async (orderCode: number): Promise<void> => {
    logger.info('Payment successful', { orderCode });
    
    const transaction = await transactionRepository.getByOrderCode(orderCode);
    if (!transaction) {
        logger.error('Transaction not found for successful payment', { orderCode });
        return;
    }
    
    const order = await orderRepository.findById(transaction.orderId.toString());
    if (!order) {
        logger.error('Order not found for successful payment', { orderCode });
        return;
    }

    await orderRepository.updateStatus(order._id, OrderStatus.PAID, {
        paidAt: new Date()
    });
    logger.info('Order status updated to PAID', { orderCode, orderId: order._id, orderCodeString: order.code });

    const qrGenerationEvent: QrGenerationRequestedEvent = {
        orderId: transaction.orderId.toString(),
        code: order.code,
        userId: order.userId,
        qrCount: order.packageSnapshot.qrCount,
        packageName: order.packageSnapshot.name,
        eventTimestamp: new Date().toISOString(),
    };

    try {
        await publishMessage(
            EventTopics.Order.QrGenerationRequested,
            qrGenerationEvent
        );
        logger.info('QR generation event published', { orderCode, qrCount: order.packageSnapshot.qrCount });
    } catch (error) {
        logger.error('Failed to publish QR generation event', { orderCode, error: String(error) });
        // Note: We don't throw here to avoid failing the webhook response
        // The order is already marked as PAID, QR generation can be retried later
    }
};

export const processWebhookAsync = async (webhook: Webhook): Promise<Result<boolean>> => {
    try {
        logger.info('Processing webhook', { webhook });
        const verifiedResult = await paymentAdapter.verifyWebhook(webhook);

        logger.info('Verified webhook data', {
            orderCode: verifiedResult.orderCode,
            paymentStatus: verifiedResult.paymentStatus
        });

        if (verifiedResult.paymentStatus === PaymentStatus.Success) {
            await handlePaymentSuccessAsync(verifiedResult.orderCode);
        }

        return success(true);
    } catch (error) {
        logger.error('Failed to process webhook', { error });
        return failure({
            kind: 'Internal',
            code: 'WEBHOOK_PROCESSING_FAILED',
            message: error instanceof Error ? error.message : 'Failed to process webhook',
            cause: error
        });
    }
}