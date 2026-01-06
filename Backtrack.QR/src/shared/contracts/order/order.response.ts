export type OrderResponse = {
    id: string;
    packageSnapshot: {
        name: string;
        qrCount: number;
        price: number;
    };
    status: string;
    shippingAddress: string;
    totalAmount: number;
    createdAt: string;
}