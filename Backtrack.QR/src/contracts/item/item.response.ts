import type { QrCodeResponse } from '@/src/contracts/qr-code/qr-code.response.js';

export type ItemResponse = {
    id: string;
    name: string;
    description?: string;
    imageUrls?: string[];
    ownerId: string;
    qrCode?: QrCodeResponse;
    createdAt: string;
    updatedAt: string | null;
};
