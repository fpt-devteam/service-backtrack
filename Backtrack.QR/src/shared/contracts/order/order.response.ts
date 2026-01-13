export type OrderResponse = {
    id: string;
    packageSnapshot: {
        name: string;
        qrCount: number;
        price: number;
    };
    orderCode: number;
    status: string;
    shippingAddress: string;
    totalAmount: number;
    createdAt: string;
}