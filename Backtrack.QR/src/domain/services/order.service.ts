import { IOrder, OrderStatus } from "@/src/infrastructure/database/models/order.model.js";
import { orderRepository } from "@/src/infrastructure/repositories/order.repository.js";
import { packageRepository } from "@/src/infrastructure/repositories/package.repository.js";
import { toOrderResponse } from "@/src/shared/contracts/order/order.mapper.js";
import { CreateOrderRequest } from "@/src/shared/contracts/order/order.request.js";
import { OrderResponse } from "@/src/shared/contracts/order/order.response.js";
import { PackageErrors } from "@/src/shared/errors/catalog/package.error.js";
import { toObjectIdOrNull } from "@/src/shared/utils/mongoes.object-id.util.js";
import { failure, Result, success } from "@/src/shared/utils/result.js";

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
    const orderData: Partial<IOrder> = {
    userId,
    packageId: objectPackageId,
    packageSnapshot: {  
        name: pack.name,
        qrCount: pack.qrCount,
        price: pack.price,
    },
    status: OrderStatus.PAID,
    shippingAddress: request.shippingAddress,
    totalAmount: pack.price,
    };
    const createdOrder = await orderRepository.create(orderData);
    return success(toOrderResponse(createdOrder));
};