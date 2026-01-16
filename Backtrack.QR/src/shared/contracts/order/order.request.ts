import { OrderStatus } from "@/src/infrastructure/database/models/order.model.js";

export type CreateOrderRequest = {
    packageId: string;
    shippingAddress: string;
}

export type UpdateOrderStatusRequest = {
    orderId: string;
    status: OrderStatus;
    additionalData?: any
    message?: string;
}