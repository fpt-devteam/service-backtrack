import { QrCodeModel, IQrCode, Item } from '@/src/infrastructure/database/models/qr-code.models.js';
import { IUser, UserModel } from '@/src/infrastructure/database/models/user.model.js';
import { createBaseRepo } from './common/base.repository.js';
import mongoose, { type ClientSession } from "mongoose";

const baseRepo = createBaseRepo<IQrCode, mongoose.Types.ObjectId>(QrCodeModel, (id) => new mongoose.Types.ObjectId(id));

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
): Promise<{ qrCodes: IQrCode[]; totalCount: number }> => {

  const [qrCodes, totalCount] = await Promise.all([
    QrCodeModel.find({ ownerId, deletedAt: null })
      .sort({ createdAt: -1 })
      .skip(offset)
      .limit(limit)
      .lean()
      .exec(),
    QrCodeModel.countDocuments({ ownerId, deletedAt: null })
  ]);

  return { qrCodes: qrCodes as IQrCode[], totalCount };
};

const getByPublicCodeAsync = async (
  publicCode: string
): Promise<{ qrCode: IQrCode | null, owner: IUser | null }> => {
  const qrCode = (await QrCodeModel.findOne({ publicCode, deletedAt: null })
    .lean()
    .exec()) as IQrCode | null;

  if (!qrCode) {
    return { qrCode: null, owner: null };
  }
  const owner = (await UserModel.findOne({ _id: qrCode.ownerId, deletedAt: null })
    .lean()
    .exec()) as IUser | null;

  if (!owner) {
    return { qrCode: null, owner: null };
  }

  return { qrCode, owner, };
};

const updateItemAsync = async (
  qrCodeId: mongoose.Types.ObjectId,
  itemData: {
    name?: string;
    description?: string;
    imageUrls?: string[];
  }
): Promise<IQrCode | null> => {
  const updated = await QrCodeModel.findOneAndUpdate(
    { _id: qrCodeId, deletedAt: null },
    {
      $set: {
        ...(itemData.name !== undefined && { 'item.name': itemData.name }),
        ...(itemData.description !== undefined && { 'item.description': itemData.description }),
        ...(itemData.imageUrls !== undefined && { 'item.imageUrls': itemData.imageUrls }),
        updatedAt: new Date(),
        linkedAt: new Date(),
      }
    },
    { new: true, runValidators: true }
  ).lean().exec();

  return updated as IQrCode | null;
};

export const qrCodeRepository = {
  ...baseRepo,
  existsByPublicCodeAsync,
  getAllAsync,
  getByPublicCodeAsync,
  updateItemAsync,
};
