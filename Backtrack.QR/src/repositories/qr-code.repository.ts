import { QrCode } from '@/src/database/models/qr-code.models.js';
import { Result, success, failure } from '@/src/utils/result.js';
import { Error } from '@/src/errors/error.js';

export const create = async (
    qrCode: InstanceType<typeof QrCode>
): Promise<Result<InstanceType<typeof QrCode>>> => {
    try {
        const saved = await qrCode.save();
        return success(saved);

    } catch (error: any) {
        // Check for duplicate key error (unique constraint violation)
        if (error.code === 11000) {
            return failure({
                kind: "Conflict",
                code: "DuplicatePublicCode",
                message: "QR code with this publicCode already exists",
                details: String(error)
            } as Error);
        }

        return failure({
            kind: "Internal",
            code: "CreateQrCodeError",
            message: "Failed to create QR code",
            details: String(error)
        } as Error);
    }
};

export const existsByPublicCode = async (
    publicCode: string
): Promise<Result<boolean>> => {
    try {
        const count = await QrCode.countDocuments({ publicCode, deletedAt: null });
        return success(count > 0);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "CheckQrCodeExistsError",
            message: "Failed to check if QR code exists",
            details: String(error)
        } as Error);
    }
};

export const getByItemId = async (
    itemId: string
): Promise<Result<InstanceType<typeof QrCode> | null>> => {
    try {
        const qrCode = await QrCode.findOne({ itemId, deletedAt: null });
        return success(qrCode);
    } catch (error) {
        return failure({
            kind: "Internal",
            code: "GetQrCodeByItemIdError",
            message: "Failed to get QR code by item ID",
            details: String(error)
        } as Error);
    }
};
