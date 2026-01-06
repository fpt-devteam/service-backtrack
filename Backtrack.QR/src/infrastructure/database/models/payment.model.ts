import mongoose from "mongoose";

export enum PaymentStatus {
  PENDING = 'PENDING',
  SUCCESS = 'SUCCESS',
  FAILED = 'FAILED',
  CANCELLED = 'CANCELLED'
}

export interface IPayment {
  _id: mongoose.Types.ObjectId;
  orderId: mongoose.Types.ObjectId;
  userId: string;
  amount: number;
  orderCode: number;
  status: PaymentStatus;
  transactionId?: string;
  paidAt?: Date;
  webhookData?: Record<string, any>;
  createdAt: Date;
  updatedAt: Date;
}
const PaymentSchema = new mongoose.Schema<IPayment>(
  {
    orderId: {
      type: mongoose.Schema.Types.ObjectId,
      required: true,
      ref: 'Order',
      index: true
    },
    userId: {
      type: String,
      required: true,
      ref: 'User',
      index: true
    },
    amount: {
      type: Number,
      required: true,
      min: 0
    },
    orderCode: {
      type: Number,
      required: true,
      index: true,
      unique: true
    },
    status: {
      type: String,
      enum: Object.values(PaymentStatus),
      default: PaymentStatus.PENDING,
      required: true,
      index: true
    },
    transactionId: {
      type: String,
      sparse: true,
      index: true
    },
    paidAt: {
      type: Date
    },
    webhookData: {
      type: Map,
      of: mongoose.Schema.Types.Mixed
    }
  },
  {
    timestamps: true,
    collection: 'payments'
  }
);

PaymentSchema.index({ orderId: 1, status: 1 });
PaymentSchema.index({ userId: 1, createdAt: -1 });

export const PaymentModel = mongoose.model<IPayment>('Payment', PaymentSchema);