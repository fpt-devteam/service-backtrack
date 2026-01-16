import mongoose from "mongoose";

export enum TransactionStatus {
  PENDING = 'PENDING',
  SUCCESS = 'SUCCESS',
  FAILED = 'FAILED',
}

export interface ITransaction {
  _id: mongoose.Types.ObjectId;
  orderId: mongoose.Types.ObjectId;
  
  orderCode: number;
  paymentLinkId: string;
  amount: number;
  status: TransactionStatus;
  
  webhookData?: any;
  
  createdAt: Date;
  updatedAt: Date;
}

const TransactionSchema = new mongoose.Schema<ITransaction>(
  {
    orderId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'Order',
      required: true,
      index: true,
    },

    orderCode: {
      type: Number,
      required: true,
      index: true,
    },
    paymentLinkId: {
      type: String,
      required: true,
      unique: true,
      index: true,
    },

    amount: {
      type: Number,
      required: true,
      min: 0,
    },

    status: {
      type: String,
      enum: Object.values(TransactionStatus),
      required: true,
      default: TransactionStatus.PENDING,
    },

    webhookData: {
      type: mongoose.Schema.Types.Mixed,
      default: null,
    },
  },
  {
    timestamps: true,
  }
);

TransactionSchema.index({ orderId: 1, createdAt: -1 });
TransactionSchema.index({ status: 1, createdAt: -1 }); 
TransactionSchema.index({ paymentLinkId: 1 }, { unique: true })
TransactionSchema.index({ orderCode: 1 }, { unique: true })
export const TransactionModel = mongoose.model<ITransaction>(
  'Transaction',
  TransactionSchema
);