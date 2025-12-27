import { IQrCode, QrCode, type IItem } from '@/src/database/models/qr-code.models.js';
import { qrCodeRepository } from '@/src/repositories/qr-code.repository.js';
import { Result, success, failure } from '@/src/utils/result.js';
import { QrCodeErrors } from '@/src/errors/catalog/qr-code.error.js';
import { generatePublicCode } from '@/src/utils/qr-code-generator.js';
import { toObjectIdOrNull } from '@/src/utils/object-id.js';
import { WithId } from '../repositories/common/base.repository.js';
import { IUser } from '../database/models/user.model.js';
import { userRepository } from '../repositories/user.repository.js';
import * as loggers from '@/src/utils/logger.js';
import { log } from 'node:console';
import { UserErrors } from '../errors/catalog/user.error.js';

const MAX_RETRIES = 5;
export const createAsync = async (
    itemData: IItem,
    ownerId: string
): Promise<Result<WithId<IQrCode>>> => {

    let publicCode = "";
    for (let attempts = 0; attempts < MAX_RETRIES; attempts++) {
        publicCode = generatePublicCode();

        const exists = await qrCodeRepository.existsByPublicCodeAsync(publicCode);
        if (!exists) break;

        if (attempts === MAX_RETRIES - 1) {
            return failure({
                kind: "Internal",
                code: "QrCodeGenerationFailed",
                message: "Failed to generate unique QR code after maximum retries"
            });
        }
    }

    const qrCode = new QrCode({
        publicCode,
        ownerId,
        item: itemData,
        linkedAt: new Date(),
    });

    return success(await qrCodeRepository.create(qrCode));

};

export const getAllAsync = async (
    ownerId: string,
    page: number,
    pageSize: number
): Promise<Result<{ qrCodes: WithId<IQrCode>[]; totalCount: number }>> => {
    const result = await qrCodeRepository.getAllAsync(ownerId, (page - 1) * pageSize, pageSize);
    return success(result);
};

export const getByIdAsync = async (
    id: string
): Promise<Result<{ qrCode: WithId<IQrCode>, owner: WithId<IUser, string> }>> => {
    const objectId = toObjectIdOrNull(id);
    loggers.error(`Converting ID to ObjectId: ${id} -> ${objectId}`);
    if (objectId === null) {
        return failure(QrCodeErrors.NotFound);
    }
    loggers.error(`Getting QR code by ID: ${id}`);

    const qrCode = await qrCodeRepository.findById(objectId);

    if (qrCode === null) {
        return failure(QrCodeErrors.NotFound);
    }

    loggers.error(`Found QR code: ${qrCode._id.toString()}, fetching owner with ID: ${qrCode.ownerId}`);


    const owner = await userRepository.findById(qrCode.ownerId);
    if (owner === null) {
        return failure(UserErrors.NotFound);
    }

    loggers.debug(`Found owner: ${owner._id.toString()} for QR code: ${qrCode._id.toString()}`);

    return success({ qrCode, owner });
};

export const getByPublicCodeAsync = async (
    publicCode: string
): Promise<Result<{ qrCode: WithId<IQrCode>, owner: WithId<IUser> }>> => {
    const { qrCode, owner } = await qrCodeRepository.getByPublicCodeAsync(publicCode);

    if (!qrCode || !owner) {
        return failure(QrCodeErrors.NotFound);
    }

    return success({ qrCode, owner });
};

export const updateItemAsync = async (
    qrCodeId: string,
    itemData: IItem
): Promise<Result<WithId<IQrCode>>> => {
    const objectId = toObjectIdOrNull(qrCodeId);
    if (objectId === null) {
        return failure(QrCodeErrors.NotFound);
    }

    const updated = await qrCodeRepository.updateItemAsync(objectId, itemData);

    if (updated === null) {
        return failure(QrCodeErrors.NotFound);
    }

    return success(updated);
};
