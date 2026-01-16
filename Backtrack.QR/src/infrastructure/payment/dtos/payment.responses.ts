export interface PaymentLinkResponse {
  orderCode: number;
  checkoutUrl: string;
  qrCode: string;
  paymentLinkId: string;
}

export interface PaymentInfoResponse {
  orderCode: number;
  amount: number;
  status: string;
  transactionId?: string;
}