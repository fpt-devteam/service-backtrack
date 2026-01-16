import { Request, Response } from 'express';
import * as orderService from '@/src/domain/services/order.service.js';
import * as paymentService from '@/src/domain/services/payment.service.js';
import * as transactionService from '@/src/domain/services/transaction.service.js';
import { CreateOrderRequest } from '@/src/shared/contracts/order/order.request.js';
import { HEADER_AUTH_ID } from '@/src/shared/utils/headers.js';
import { isFailure } from '@/src/shared/utils/result.js';

export const createLinkPaymentAsync = async (req: Request, res: Response): Promise<void> => {
    const request: CreateOrderRequest = req.body;
    const ownerId = req.headers[HEADER_AUTH_ID] as string;
    const correlationId = (req as any).correlationId || 'unknown';
    const orderResult = await orderService.createAsync(request, ownerId);

    if (isFailure(orderResult)) {
        res.status(500).json({
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
        res.status(500).json({
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

    res.status(201).json({
        order,
        paymentLink: paymentResult.value
    });
}

export const handlePaymentWebhookAsync = async (req: Request, res: Response) => {
    const webhook = req.body;
    const correlationId = (req as any).correlationId || 'unknown';
    const processResult = await paymentService.processWebhookAsync(webhook);
    if (isFailure(processResult)) {
        res.status(500).json({
            error: processResult.error,
            correlationId
        });
        return;
    }
    res.status(200).json({ success: true });
}

export const getOrderByCodeAsync = async (req: Request, res: Response) => {
    const code = req.params.orderCode;
    const correlationId = (req as any).correlationId || 'unknown';
    const orderResult = await orderService.findByCodeAsync(code);
    if (isFailure(orderResult)) {
        res.status(404).json({
            error: orderResult.error,
            correlationId
        });
        return;
    }
    res.status(200).json({
        order: orderResult.value
    });
}

export const getOrderByIdAsync = async (req: Request, res: Response) => {
    const { id } = req.params;
    const correlationId = (req as any).correlationId || 'unknown';
    const orderResult = await orderService.findByIdAsync(id);
    if (isFailure(orderResult)) {
        res.status(404).json({
            error: orderResult.error,
            correlationId
        });
        return;
    }
    res.status(200).json({
        order: orderResult.value
    });
}

export const updateOrderStatusAsync = async (req: Request, res: Response) => {
    const request = req.body;
    const correlationId = (req as any).correlationId || 'unknown';
    const orderResult = await orderService.updateStatusAsync(request);
    if (isFailure(orderResult)) {
        res.status(404).json({
            error: orderResult.error,
            correlationId
        });
        return;
    }
    res.status(200).json({
        order: orderResult.value
    });
}
