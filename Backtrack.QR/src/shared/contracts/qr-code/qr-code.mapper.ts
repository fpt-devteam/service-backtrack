import type { QrCodeResponse, QrCodeWithOwnerResponse } from './qr-code.response.js';
import { IQrCode } from '@/src/infrastructure/database/models/qr-code.models.js';
import { IUser } from '@/src/infrastructure/database/models/user.model.js';

/**
 * QR Code Model -> Response
 */
export const toQrCodeWithOwnerResponse = (qrCode: IQrCode, owner: IUser): QrCodeWithOwnerResponse => ({
    qrCode: {
        id: qrCode._id.toString(),
        publicCode: qrCode.publicCode,
        linkedAt: qrCode.linkedAt?.toISOString() || null,
        createdAt: qrCode.createdAt?.toISOString() || new Date().toISOString(),
    },
    item: qrCode.item ? {
        name: qrCode.item.name,
        description: qrCode.item.description,
        imageUrls: qrCode.item.imageUrls,
    } : null,
    owner: {
        id: owner._id.toString(),
        email: owner.email || '',
        displayName: owner.displayName || null,
    }
});


export const toQrCodeResponse = (qrCode: IQrCode): QrCodeResponse => ({
    qrCode: {
        id: qrCode._id.toString(),
        publicCode: qrCode.publicCode,
        linkedAt: qrCode.linkedAt?.toISOString() || null,
        createdAt: qrCode.createdAt?.toISOString() || new Date().toISOString(),
    },
    item: qrCode.item ? {
        name: qrCode.item.name,
        description: qrCode.item.description,
        imageUrls: qrCode.item.imageUrls,
    } : null,
    ownerId: qrCode.ownerId,
});
