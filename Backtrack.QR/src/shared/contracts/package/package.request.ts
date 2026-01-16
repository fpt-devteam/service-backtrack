export type CreatePackageRequest = {
    name: string;
    price: number;
    qrCount: number;
    description?: string;
    isActive: boolean;
};

export type UpdatePackageRequest = {
    name?: string;
    price?: number;
    qrCount?: number;
    description?: string;
    isActive?: boolean;
};