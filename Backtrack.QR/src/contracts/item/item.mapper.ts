import type { ItemResponse } from '@/src/contracts/item/item.response.js';
import { Item } from '@/src/database/models/item.model.js';
import { QrCode } from '@/src/database/models/qr-code.models.js';
import { toQrCodeResponse } from '@/src/contracts/qr-code/qr-code.mapper.js';

/**
 * Item and QR Code Models -> ItemResponse
 */
export const toItemResponse = (
    item: InstanceType<typeof Item>,
    qrCode?: InstanceType<typeof QrCode>
): ItemResponse => ({
    id: item._id.toString(),
    name: item.name,
    description: item.description,
    imageUrls: item.imageUrls,
    ownerId: item.ownerId,
    qrCode: qrCode ? toQrCodeResponse(qrCode) : undefined,
    createdAt: item.createAt.toISOString(),
    updatedAt: item.updateAt?.toISOString() || null
});
