import { WithId } from '@/src/repositories/common/base.repository.js';
import type { QrCodeResponse, QrCodeWithOwnerResponse } from './qr-code.response.js';
import { IQrCode } from '@/src/database/models/qr-code.models.js';
import { IUser } from '@/src/database/models/user.model.js';

/**
 * QR Code Model -> Response
 */
export const toQrCodeWithOwnerResponse = (qrCode: WithId<IQrCode>, owner: WithId<IUser, string>): QrCodeWithOwnerResponse => ({
    qrCode: {
        id: qrCode._id.toString(),
        publicCode: qrCode.publicCode,
        linkedAt: qrCode.linkedAt?.toISOString() || null,
        createdAt: qrCode.createdAt.toISOString(),
    },
    item: qrCode.item ? {
        name: qrCode.item.name,
        description: qrCode.item.description,
        imageUrls: qrCode.item.imageUrls,
    } : null,
    owner: {
        id: owner._id.toString(),
        email: owner.email,
        displayName: owner.displayName || null,
    }
});


export const toQrCodeResponse = (qrCode: WithId<IQrCode>): QrCodeResponse => ({
    qrCode: {
        id: qrCode._id.toString(),
        publicCode: qrCode.publicCode,
        linkedAt: qrCode.linkedAt?.toISOString() || null,
        createdAt: qrCode.createdAt.toISOString(),
    },
    item: qrCode.item ? {
        name: qrCode.item.name,
        description: qrCode.item.description,
        imageUrls: qrCode.item.imageUrls,
    } : null,
    ownerId: qrCode.ownerId,
});
