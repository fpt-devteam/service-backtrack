export type PackageResponse = {
    id: string;
    name: string;
    price: number;
    qrCount: number;
    description?: string;
    isActive: boolean;
    createdAt: string;
    deletedAt: string | null;
};