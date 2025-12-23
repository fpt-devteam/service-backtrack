import type { QrCodeResponse } from './qr-code.response.js';
import { QrCode } from '@/src/database/models/qr-code.models.js';

/**
 * QR Code Model -> Response
 */
export const toQrCodeResponse = (qrCode: InstanceType<typeof QrCode>): QrCodeResponse => ({
    id: qrCode._id.toString(),
    publicCode: qrCode.publicCode,
    status: qrCode.status,
    itemId: qrCode.itemId,
    ownerId: qrCode.ownerId,
    linkedAt: qrCode.linkedAt?.toISOString(),
    createdAt: qrCode.createAt.toISOString()
});
