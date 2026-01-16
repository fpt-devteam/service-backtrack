export type OrderResponse = {
    id: string;
    packageSnapshot: {
        name: string;
        qrCount: number;
        price: number;
    };
    code: string;
    status: string;
    shippingAddress: string;
    totalAmount: number;
    createdAt: string;
}