import mongoose from "mongoose";

export enum TransactionStatus {
  PENDING = 'PENDING',
  SUCCESS = 'SUCCESS',
  FAILED = 'FAILED',
}

export interface ITransaction {
  _id: mongoose.Types.ObjectId;
  orderId: mongoose.Types.ObjectId;
  
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

export const TransactionModel = mongoose.model<ITransaction>(
  'Transaction',
  TransactionSchema
);