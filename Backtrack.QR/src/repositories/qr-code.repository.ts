import { QrCode, type IItem, IQrCode } from '@/src/database/models/qr-code.models.js';
import { IUser, User } from '@/src/database/models/user.model.js';
import { createBaseRepo, WithId } from './common/base.repository.js';
import mongoose, { type ClientSession } from "mongoose";

const baseRepo = createBaseRepo<IQrCode, mongoose.Types.ObjectId>(QrCode, (id) => new mongoose.Types.ObjectId(id));

const existsByPublicCodeAsync = async (
    publicCode: string,
    session?: ClientSession
): Promise<boolean> => {
    return await baseRepo.exists({ publicCode }, session);
};

const getAllAsync = async (
    ownerId: string,
    offset: number,
    limit: number
): Promise<{ qrCodes: WithId<IQrCode>[]; totalCount: number }> => {

    const [qrCodes, totalCount] = await Promise.all([
        QrCode.find({ ownerId, deletedAt: null })
            .sort({ createdAt: -1 })
            .skip(offset)
            .limit(limit)
            .lean()
            .exec(),
        QrCode.countDocuments({ ownerId, deletedAt: null })
    ]);

    return { qrCodes: qrCodes as WithId<IQrCode>[], totalCount };
};

const getByPublicCodeAsync = async (
    publicCode: string
): Promise<{ qrCode: WithId<IQrCode> | null, owner: WithId<IUser> | null }> => {
    const qrCode = (await QrCode.findOne({ publicCode, deletedAt: null })
        .lean()
        .exec()) as WithId<IQrCode> | null;

    if (!qrCode) {
        return { qrCode: null, owner: null };
    }
    const owner = (await User.findOne({ _id: qrCode.ownerId, deletedAt: null })
        .lean()
        .exec()) as WithId<IUser> | null;

    if (!owner) {
        return { qrCode: null, owner: null };
    }

    return { qrCode, owner, };
};

const updateItemAsync = async (
    qrCodeId: mongoose.Types.ObjectId,
    itemData: IItem
): Promise<WithId<IQrCode> | null> => {
    const updated = await QrCode.findOneAndUpdate(
        { _id: qrCodeId, deletedAt: null },
        {
            $set: {
                item: itemData,
                linkedAt: new Date(),
            }
        },
        { new: true, runValidators: true }
    ).lean().exec();

    return updated as WithId<IQrCode> | null;
};

export const qrCodeRepository = {
    ...baseRepo,
    existsByPublicCodeAsync,
    getAllAsync,
    getByPublicCodeAsync,
    updateItemAsync,
};
