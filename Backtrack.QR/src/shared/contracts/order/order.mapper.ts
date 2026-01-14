import { IOrder } from "@/src/infrastructure/database/models/order.model.js";
import { OrderResponse } from "./order.response.js";

export const toOrderResponse = (order: IOrder): OrderResponse => {
    return {
        id: order._id.toString(),
        orderCode: order.orderCode,
        packageSnapshot: {
            name: order.packageSnapshot.name,
            qrCount: order.packageSnapshot.qrCount,
            price: order.packageSnapshot.price,
        },
        status: order.status,
        shippingAddress: order.shippingAddress,
        totalAmount: order.totalAmount,
        createdAt: order.createdAt.toISOString(),
    };
}