import { CreatePaymentLinkRequest } from "./dtos/payment.requests.js";
import { payosClient } from "./payos.js";


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
      qrCode: response.qrCode
    };
}

export const paymentAdapter = {
    createPaymentLink
};
