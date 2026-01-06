export interface CreatePaymentLinkRequest {
  orderCode: number;
  amount: number;
  description: string;
  returnUrl: string;
  cancelUrl: string;
}

export interface CancelPaymentRequest {
  orderCode: number;
  reason?: string;
}