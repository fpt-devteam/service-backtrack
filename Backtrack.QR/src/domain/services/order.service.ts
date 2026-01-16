import { IOrder, OrderStatus } from "@/src/infrastructure/database/models/order.model.js";
import { orderRepository } from "@/src/infrastructure/repositories/order.repository.js";
import { packageRepository } from "@/src/infrastructure/repositories/package.repository.js";
import { toOrderResponse } from "@/src/shared/contracts/order/order.mapper.js";
import { CreateOrderRequest, UpdateOrderStatusRequest } from "@/src/shared/contracts/order/order.request.js";
import { OrderResponse } from "@/src/shared/contracts/order/order.response.js";
import { PackageErrors } from "@/src/shared/errors/catalog/package.error.js";
import { toObjectIdOrNull } from "@/src/shared/utils/mongoes.object-id.util.js";
import { failure, Result, success } from "@/src/shared/utils/result.js";
import { generateOrderCodeSystem } from "@/src/shared/utils/order-code.js";
import { OrderErrors } from "@/src/shared/errors/catalog/order.error.js";

export const createAsync = async (
  request: CreateOrderRequest,
  userId: string
): Promise<Result<OrderResponse>> => {
    const objectPackageId = toObjectIdOrNull(request.packageId);
    if ((objectPackageId === null)) {
        return failure(PackageErrors.NotFound);
    }
    const pack = await packageRepository.findById(objectPackageId);
    if (!pack) {
        return failure(PackageErrors.NotFound);
    }
    const code = generateOrderCodeSystem(userId);
    const orderData: Partial<IOrder> = {
    userId,
    code,
    packageId: objectPackageId,
    packageSnapshot: {
        name: pack.name,
        qrCount: pack.qrCount,
        price: pack.price,
    },
    shippingAddress: request.shippingAddress,
    totalAmount: pack.price,
    };
    const createdOrder = await orderRepository.create(orderData);
    return success(toOrderResponse(createdOrder));
};

export const findByCodeAsync = async (code: string): Promise<Result<OrderResponse>> => {
    const order = await orderRepository.findByCode(code);
    if (!order) {
        return failure(OrderErrors.NotFound);
    }
    return success(toOrderResponse(order));
};

export const findByIdAsync = async (id: string): Promise<Result<OrderResponse>> => {
    const order = await orderRepository.findById(id);
    if (!order) {
        return failure(OrderErrors.NotFound);
    }
    return success(toOrderResponse(order));
};

export const updateStatusAsync = async (
    request: UpdateOrderStatusRequest,
): Promise<Result<OrderResponse>> => {
    const orderObjectId = toObjectIdOrNull(request.orderId);
    if (orderObjectId === null) {
        return failure(OrderErrors.NotFound);
    }
    const updatedOrder = await orderRepository.updateStatus(orderObjectId, request.status,{ cancelReason: request.message });
    if (!updatedOrder) {
        return failure(OrderErrors.NotFound);
    }
    return success(toOrderResponse(updatedOrder));
};