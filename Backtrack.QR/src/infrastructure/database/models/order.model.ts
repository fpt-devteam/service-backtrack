import mongoose from "mongoose";

export enum OrderStatus {
  UNPAID = 'UNPAID',
  PAID ='PAID',            
  PROCESSING = 'PROCESSING', 
  SHIPPING = 'SHIPPING',     
  DELIVERED = 'DELIVERED',
  CANCELLED = 'CANCELLED',
  
}
export interface IOrder {
  _id: mongoose.Types.ObjectId;
  userId: string;
  code: string;

  packageId: mongoose.Types.ObjectId;
  packageSnapshot: {
    name: string;
    qrCount: number;
    price: number;
  };

  status: OrderStatus;
  cancelReason?: string;
  
  paidAt?: Date;
  
  shippingCode?: string;
  shippingAddress: string;
  shippedAt?: Date;
  deliveredAt?: Date;
  shippingFee?: number;

  printedAt?: Date;
  printedBy?: mongoose.Types.ObjectId;
  
  totalAmount: number;
  createdAt: Date;
  updatedAt: Date;
}
const OrderSchema = new mongoose.Schema<IOrder>(
  {
    userId: { type: String, required: true, index: true, ref: 'User' },
    code: { type: String, required: true, unique: true, index: true },
    packageId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'Package',
      required: true
    },

    packageSnapshot: {
      type: {
        name: { type: String, required: true },
        qrCount: { type: Number, required: true, min: 1 },
        price: { type: Number, required: true, min: 0 },
      },
      required: true,
    },
    cancelReason: { type: String, default: null },
    status: { type: String, enum: Object.values(OrderStatus), default: OrderStatus.UNPAID },
    paidAt: { type: Date, default: null },
    shippingCode: { type: String, default: null },
    shippingAddress: { type: String, required: true },
    shippedAt: { type: Date, default: null },
    deliveredAt: { type: Date, default: null },
    shippingFee: { 
      type: Number, 
      min: 0,
      default: 0  
    },

    printedAt: {
      type: Date,
      default: null,
    },

    printedBy: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
      default: null,
    },
  
    totalAmount: { 
      type: Number, 
      required: true, 
      min: 0 
    },
    },  
    {
        timestamps: true,
    }
);

OrderSchema.index({ userId: 1, status: 1 });
OrderSchema.index({ userId: 1, createdAt: -1 });
OrderSchema.index({ status: 1, createdAt: -1 });

OrderSchema.index({ packageId: 1 });              
OrderSchema.index({ paymentId: 1 });             
OrderSchema.index({ shippingCode: 1 });         
OrderSchema.index({ status: 1, paidAt: -1 });    
OrderSchema.index({ code: 1 }, { unique: true });
OrderSchema.index({ userId: 1, status: 1, createdAt: -1 }); 

export const OrderModel = mongoose.model<IOrder>('Order', OrderSchema);