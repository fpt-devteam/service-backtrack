import { Item } from '@/src/database/models/item.model.js';
import { QrCode } from '@/src/database/models/qr-code.models.js';
import * as itemRepo from '@/src/repositories/item.repository.js';
import * as qrCodeRepo from '@/src/repositories/qr-code.repository.js';
import { Result, success, failure, isFailure } from '@/src/utils/result.js';
import { generatePublicCode } from '@/src/utils/qr-code-generator.js';
import { ItemErrors } from '@/src/errors/catalog/item.error.js';
import mongoose from 'mongoose';
import type { ItemWithQrResult, ListItemsResult } from '@/src/database/view/item.view.js';

const MAX_RETRIES = 5;

const isValidItemName = (name: string): boolean => {
    return Boolean(name && name.trim().length > 0);
};

export const getByIdAsync = async (
    id: string
): Promise<Result<ItemWithQrResult>> => {
    const itemResult = await itemRepo.getById(id);

    if (isFailure(itemResult)) {
        return itemResult;
    }

    if (itemResult.value === null) {
        return failure(ItemErrors.NotFound);
    }

    // Get the associated QR code
    const qrCodeResult = await qrCodeRepo.getByItemId(id);

    if (isFailure(qrCodeResult)) {
        return qrCodeResult;
    }

    if (qrCodeResult.value === null) {
        return failure({
            kind: "NotFound",
            code: "QrCodeNotFound",
            message: "QR code not found for this item",
            details: `Item ID: ${id}`
        });
    }

    return success({
        item: itemResult.value,
        qrCode: qrCodeResult.value
    });
};

export const getAllAsync = async (
    ownerId: string,
    page: number,
    pageSize: number
): Promise<Result<ListItemsResult>> => {
    const result = await itemRepo.getAll(ownerId, page, pageSize);

    if (isFailure(result)) {
        return result;
    }

    return success(result.value);
};

/**
 * Creates an Item and QR code together in a transaction
 * The QR code will be linked to the item immediately
 */
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
    session.startTransaction();

    try {
        // Create the item
        const item = new Item({
            name: itemData.name.trim(),
            description: itemData.description,
            imageUrls: itemData.imageUrls,
            ownerId
        });

        const itemResult = await itemRepo.create(item);
        if (isFailure(itemResult)) {
            await session.abortTransaction();
            return itemResult;
        }

        // Generate a unique public code with retries
        let publicCode: string;
        let attempts = 0;
        let codeExists = true;

        while (codeExists && attempts < MAX_RETRIES) {
            publicCode = generatePublicCode();
            const existsResult = await qrCodeRepo.existsByPublicCode(publicCode);

            if (isFailure(existsResult)) {
                await session.abortTransaction();
                return existsResult;
            }

            codeExists = existsResult.value;
            attempts++;
        }

        if (codeExists) {
            await session.abortTransaction();
            return failure({
                kind: "Internal",
                code: "QrCodeGenerationFailed",
                message: "Failed to generate unique QR code after maximum retries",
                details: `Tried ${MAX_RETRIES} times`
            });
        }

        // Create the QR code linked to the item
        const qrCode = new QrCode({
            publicCode: publicCode!,
            ownerId,
            itemId: itemResult.value._id.toString(),
            status: 'linked',
            linkedAt: new Date()
        });

        const qrResult = await qrCodeRepo.create(qrCode);
        if (isFailure(qrResult)) {
            await session.abortTransaction();
            return qrResult;
        }

        await session.commitTransaction();

        return success({
            item: itemResult.value,
            qrCode: qrResult.value
        });

    } catch (error) {
        await session.abortTransaction();
        return failure({
            kind: "Internal",
            code: "CreateItemError",
            message: "Failed to create item with QR code",
            details: String(error)
        });
    } finally {
        session.endSession();
    }
}

