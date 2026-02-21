import { model, Schema, Types, Document } from 'mongoose';
import { PaymentStatus, PaymentStatusType } from '@/src/domain/constants/payment-status.constant.js';

export interface PaymentHistoryDocument extends Document {
  _id: Types.ObjectId;
  userId: string;
  providerInvoiceId: string;
  amount: number;
  currency: string;
  status: PaymentStatusType;
  paymentDate: Date;
  createdAt: Date;
  updatedAt: Date;
}

const paymentHistorySchema = new Schema<PaymentHistoryDocument>(
  {
    userId: { type: String, required: true, index: true },
    providerInvoiceId: { type: String, required: true },
    amount: { type: Number, required: true },
    currency: { type: String, required: true },
    status: { type: String, enum: Object.values(PaymentStatus), required: true },
    paymentDate: { type: Date, required: true },
  },
  { timestamps: true }
);

export const PaymentHistoryModel = model<PaymentHistoryDocument>('PaymentHistory', paymentHistorySchema);
