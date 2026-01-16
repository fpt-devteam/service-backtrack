import { IOrder } from "@/src/infrastructure/database/models/order.model.js";
import { OrderResponse } from "./order.response.js";
import { toVietnamISOStringOrDefault } from "@/src/shared/utils/timezone.js";

export const toOrderResponse = (order: IOrder): OrderResponse => {
    return {
        id: order._id.toString(),
        code: order.code,
        packageSnapshot: {
            name: order.packageSnapshot.name,
            qrCount: order.packageSnapshot.qrCount,
            price: order.packageSnapshot.price,
        },
        status: order.status,
        shippingAddress: order.shippingAddress,
        totalAmount: order.totalAmount,
        createdAt: toVietnamISOStringOrDefault(order.createdAt),
    };
}