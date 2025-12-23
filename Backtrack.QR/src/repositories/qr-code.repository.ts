import { QrCode } from '@/src/database/models/qr-code.models.js';
import type { ClientSession } from "mongoose";

export const createAsync = async (
    qrCode: InstanceType<typeof QrCode>,
    session?: ClientSession
): Promise<InstanceType<typeof QrCode>> => await qrCode.save({ session });

export const existsByPublicCodeAsync = async (publicCode: string, session?: ClientSession) => {
    const found = await QrCode.exists({ publicCode, deletedAt: null }).session(session ?? null);
    return Boolean(found);
};

export const getByItemIdAsync = async (
    itemId: string,
): Promise<InstanceType<typeof QrCode> | null> => await QrCode.findOne({ itemId, deletedAt: null });
