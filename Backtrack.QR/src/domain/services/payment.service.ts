import { env } from "@/src/shared/configs/env.js";
import { paymentAdapter, PaymentStatus } from "@/src/infrastructure/payment/payos.adapter.js";
import { Result, success, failure } from "@/src/shared/utils/result.js";
import { PaymentLinkResponse } from "@/src/infrastructure/payment/dtos/payment.responses.js";
import * as logger from "@/src/shared/utils/logger.js";
import { Webhook } from "@payos/node";
import { orderRepository } from "@/src/infrastructure/repositories/order.repository.js";
import { OrderStatus } from "@/src/infrastructure/database/models/order.model.js";

export const createLinkPaymentAsync = async (packageName: string, amount: number, orderCode: number): Promise<Result<PaymentLinkResponse>> => {
    try {
        const result = await paymentAdapter.createPaymentLink({
            orderCode,
            amount,
            description: `${packageName}`,
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

const handlePaymentFailAsync = async (orderCode: number): Promise<void> => {
    logger.warn('Payment failed', { orderCode });

    const order = await orderRepository.findByOrderCode(orderCode);
    if (!order) {
        logger.error('Order not found for failed payment', { orderCode });
        return;
    }

    await orderRepository.updateStatus(orderCode, OrderStatus.CANCELLED, {
        cancelReason: 'Payment failed'
    });

    logger.info('Order status updated to CANCELLED', { orderCode });
};

const handlePaymentSuccessAsync = async (orderCode: number): Promise<void> => {
    logger.info('Payment successful', { orderCode });

    const order = await orderRepository.findByOrderCode(orderCode);
    if (!order) {
        logger.error('Order not found for successful payment', { orderCode });
        return;
    }

    await orderRepository.updateStatus(orderCode, OrderStatus.PAID, {
        paidAt: new Date()
    });

    logger.info('Order status updated to PAID', { orderCode });
};

export const processWebhookAsync = async (webhook: Webhook): Promise<Result<boolean>> => {
    try {
        logger.info('Processing webhook', { webhook });
        const verifiedResult = await paymentAdapter.verifyWebhook(webhook);

        logger.info('Verified webhook data', {
            orderCode: verifiedResult.orderCode,
            paymentStatus: verifiedResult.paymentStatus
        });

        if (verifiedResult.paymentStatus === PaymentStatus.Failed) {
            await handlePaymentFailAsync(verifiedResult.orderCode);
        } else if (verifiedResult.paymentStatus === PaymentStatus.Success) {
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