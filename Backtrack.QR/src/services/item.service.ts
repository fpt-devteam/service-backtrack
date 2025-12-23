import { Item } from '@/src/database/models/item.model.js';
import { QrCode } from '@/src/database/models/qr-code.models.js';
import * as itemRepo from '@/src/repositories/item.repository.js';
import * as qrCodeRepo from '@/src/repositories/qr-code.repository.js';
import { Result, success, failure } from '@/src/utils/result.js';
import { generatePublicCode } from '@/src/utils/qr-code-generator.js';
import { ItemErrors } from '@/src/errors/catalog/item.error.js';
import mongoose from 'mongoose';
import type { ItemWithQrResult, ListItemsResult } from '@/src/database/view/item.view.js';
import { QrCodeErrors } from '../errors/catalog/qr-code.error.js';
import { toObjectIdOrNull } from '@/src/utils/object-id.js';

const MAX_RETRIES = 5;

const isValidItemName = (name: string): boolean => {
    return Boolean(name && name.trim().length > 0);
};

export const getByIdAsync = async (
    id: string
): Promise<Result<ItemWithQrResult>> => {
    const objectId = toObjectIdOrNull(id);
    if (objectId === null) {
        return failure(ItemErrors.NotFound);
    }
    const item = await itemRepo.getByIdAsync(objectId);

    if (item === null) {
        return failure(ItemErrors.NotFound);
    }

    const qrCode = await qrCodeRepo.getByItemIdAsync(id);

    if (qrCode === null) {
        return failure(QrCodeErrors.NotFound);
    }

    return success({
        item,
        qrCode
    } as ItemWithQrResult);
};

export const getAllAsync = async (
    ownerId: string,
    page: number,
    pageSize: number
): Promise<Result<ListItemsResult>> => {
    const result = await itemRepo.getAllAsync(ownerId, page, pageSize);
    return success(result);
};

export const createAsync = async (
    itemData: {
        name: string;
        description?: string;
        imageUrls?: string[];
    },
    ownerId: string
): Promise<Result<ItemWithQrResult>> => {
    if (!isValidItemName(itemData.name)) {
        return failure(ItemErrors.InvalidName);
    }

    const session = await mongoose.startSession();

    try {
        let savedItem: any;
        let savedQrCode: any;

        await session.withTransaction(async () => {
            // 1) create item (MUST pass session)
            const item = new Item({
                name: itemData.name.trim(),
                description: itemData.description,
                imageUrls: itemData.imageUrls,
                ownerId,
            });

            savedItem = await itemRepo.createAsync(item, session);

            // 2) generate unique public code with retries
            let publicCode = "";
            for (let attempts = 0; attempts < MAX_RETRIES; attempts++) {
                publicCode = generatePublicCode();

                // MUST pass session
                const exists = await qrCodeRepo.existsByPublicCodeAsync(publicCode, session);
                if (!exists) break;

                if (attempts === MAX_RETRIES - 1) {
                    return failure({
                        kind: "Internal",
                        code: "QrCodeGenerationFailed",
                        message: "Failed to generate unique QR code after maximum retries"
                    });
                }
            }

            // 3) create qrCode linked to item (MUST pass session)
            const qrCode = new QrCode({
                publicCode,
                ownerId,
                itemId: savedItem._id.toString(),
                status: "linked",
                linkedAt: new Date(),
            });

            savedQrCode = await qrCodeRepo.createAsync(qrCode, session);
        });

        // transaction committed here
        return success({ item: savedItem, qrCode: savedQrCode });
    }
    finally {
        await session.endSession();
    }
};