import { Webhook, WebhookData } from "@payos/node";
import { CreatePaymentLinkRequest } from "./dtos/payment.requests.js";
import { payosClient } from "./payos.js";

export enum PaymentStatus {
    Success = 'Success',
    Failed = 'Failed',
    Pending = 'Pending'
}

export interface VerifiedWebhookResult {
    orderCode: number;
    paymentStatus: PaymentStatus;
    webhookData: WebhookData;
}

const createPaymentLink = async (
    request: CreatePaymentLinkRequest
) =>{
    const payosRequest: CreatePaymentLinkRequest = {
      orderCode: request.orderCode,
      amount: request.amount,
      description: request.description,
      returnUrl: request.returnUrl,
      cancelUrl: request.cancelUrl
    };
    const response = await payosClient.paymentRequests.create(payosRequest);
    return {
      orderCode: response.orderCode,
      checkoutUrl: response.checkoutUrl,
      qrCode: response.qrCode,
      paymentLinkId: response.paymentLinkId
    };
}

const parsePaymentStatus = (code: string): PaymentStatus => {
    if (code === '00') {
        return PaymentStatus.Success;
    } else if (code === '01' || code === '02') {
        return PaymentStatus.Failed;
    }
    return PaymentStatus.Pending;
};

const verifyWebhook = async (webhook: Webhook): Promise<VerifiedWebhookResult> => {
    const verifiedData = await payosClient.webhooks.verify(webhook);
    const paymentStatus = parsePaymentStatus(verifiedData.code);

    return {
        orderCode: verifiedData.orderCode,
        paymentStatus,
        webhookData: verifiedData
    };
};

export const paymentAdapter = {
    createPaymentLink,
    verifyWebhook
};
