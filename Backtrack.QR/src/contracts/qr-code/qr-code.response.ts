export type QrCodeResponse = {
    id: string;
    publicCode: string;
    status: string;
    itemId?: string | null;
    ownerId: string;
    linkedAt?: string | null;
    createdAt: string;
};
